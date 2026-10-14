namespace Acme.Workforce.Absence.LeaveRequest.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request snapshot calculation.</summary>
public interface ILeaveRequestSnapshotFactor
{
    string Describe();
    double Adjust(LeaveRequestSnapshot item, double valueValue);
}

public sealed record RoundingLeaveRequestSnapshotAdjuster(double Step) : ILeaveRequestSnapshotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLeaveRequestSnapshotAdjuster(double Minimum) : ILeaveRequestSnapshotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLeaveRequestSnapshotAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestSnapshot item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record FlaggedLeaveRequestSnapshotAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLeaveRequestSnapshotAdjuster(double Percent) : ILeaveRequestSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLeaveRequestSnapshotAdjuster(double Cap) : ILeaveRequestSnapshotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the leave request snapshot steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestSnapshotEngine(List<ILeaveRequestSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestSnapshotEngine DefaultLeaveRequestSnapshotEngine(Registry flags)
    {
        return new LeaveRequestSnapshotEngine(
            new List<ILeaveRequestSnapshotFactor>
            {
                new RoundingLeaveRequestSnapshotAdjuster(0.25),
                new MinimumLeaveRequestSnapshotAdjuster(20.4),
                new TieredLeaveRequestSnapshotAdjuster(
                    new List<double> { 591.0, 6094.0, 7777.0 },
                    new List<double> { 0.88, 0.82, 1.01 }),
                new FlaggedLeaveRequestSnapshotAdjuster(flags, "leave-request-snapshot-batched", 2.1),
                new PercentLeaveRequestSnapshotAdjuster(5.5),
                new CapLeaveRequestSnapshotAdjuster(38409.0),
            },
            new List<string>(),
            0.4,
            25929.4);
    }

    public double Price(LeaveRequestSnapshot item)
    {
        var current = item.WeightedValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
