namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled leave request snapshot calculation.</summary>
public interface IScheduledLeaveRequestSnapshotAdjuster
{
    string Describe();
    double Adjust(ScheduledLeaveRequestSnapshot item, double valueValue);
}

public sealed record ScheduledLeaveRequestSnapshotRatioOffsetAdjuster(double Weight) : IScheduledLeaveRequestSnapshotAdjuster
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(ScheduledLeaveRequestSnapshot item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

public sealed record RoundingScheduledLeaveRequestSnapshotAdjuster(double Step) : IScheduledLeaveRequestSnapshotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledLeaveRequestSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScheduledLeaveRequestSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IScheduledLeaveRequestSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledLeaveRequestSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledLeaveRequestSnapshotAdjuster(double Minimum) : IScheduledLeaveRequestSnapshotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledLeaveRequestSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledLeaveRequestSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IScheduledLeaveRequestSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledLeaveRequestSnapshot item, double valueValue)
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

public sealed record CapScheduledLeaveRequestSnapshotAdjuster(double Cap) : IScheduledLeaveRequestSnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledLeaveRequestSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled leave request snapshot steps in order and keeps a trace of each.</summary>
public sealed class ScheduledLeaveRequestSnapshotEngine(List<IScheduledLeaveRequestSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledLeaveRequestSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledLeaveRequestSnapshotEngine DefaultScheduledLeaveRequestSnapshotEngine(Registry flags)
    {
        return new ScheduledLeaveRequestSnapshotEngine(
            new List<IScheduledLeaveRequestSnapshotAdjuster>
            {
                new ScheduledLeaveRequestSnapshotRatioOffsetAdjuster(0.2),
                new RoundingScheduledLeaveRequestSnapshotAdjuster(0.05),
                new FlaggedScheduledLeaveRequestSnapshotAdjuster(
                    flags,
                    "scheduled-leave-request-snapshot-weighted",
                    0.9),
                new MinimumScheduledLeaveRequestSnapshotAdjuster(20.8),
                new TieredScheduledLeaveRequestSnapshotAdjuster(
                    new List<double> { 1023.0, 2586.0, 8639.0 },
                    new List<double> { 1.07, 1.1, 1.09 }),
                new CapScheduledLeaveRequestSnapshotAdjuster(1801.0),
            },
            new List<string>(),
            2.2,
            43206.6);
    }

    public double Price(ScheduledLeaveRequestSnapshot item)
    {
        var current = item.EffectiveVolume();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<ScheduledLeaveRequestSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
