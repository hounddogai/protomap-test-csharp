namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request window calculation.</summary>
public interface ILeaveRequestWindowAdjuster
{
    string Describe();
    double Adjust(LeaveRequestWindow item, double valueValue);
}

public sealed record CapLeaveRequestWindowAdjuster(double Cap) : ILeaveRequestWindowAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLeaveRequestWindowAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestWindowAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestWindow item, double valueValue)
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

public sealed record FlaggedLeaveRequestWindowAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLeaveRequestWindowAdjuster(double Step) : ILeaveRequestWindowAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLeaveRequestWindowAdjuster(double Minimum) : ILeaveRequestWindowAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLeaveRequestWindowAdjuster(double Percent) : ILeaveRequestWindowAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request window steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestWindowEngine(List<ILeaveRequestWindowAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestWindowAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestWindowEngine DefaultLeaveRequestWindowEngine(Registry flags)
    {
        return new LeaveRequestWindowEngine(
            new List<ILeaveRequestWindowAdjuster>
            {
                new CapLeaveRequestWindowAdjuster(2432.0),
                new TieredLeaveRequestWindowAdjuster(
                    new List<double> { 1377.0, 1620.0, 3350.0, 9328.0 },
                    new List<double> { 0.89, 1.0, 1.15, 0.89 }),
                new FlaggedLeaveRequestWindowAdjuster(flags, "leave-request-window-lenient", 8.1),
                new RoundingLeaveRequestWindowAdjuster(1.0),
                new MinimumLeaveRequestWindowAdjuster(12.2),
                new PercentLeaveRequestWindowAdjuster(9.2),
            },
            new List<string>(),
            0.2,
            15447.1);
    }

    public double Compute(LeaveRequestWindow item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
