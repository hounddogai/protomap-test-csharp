namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled leave request baseline calculation.</summary>
public interface IScheduledLeaveRequestBaselineAdjuster
{
    string Describe();
    double Adjust(ScheduledLeaveRequestBaseline item, double valueValue);
}

public sealed record TieredScheduledLeaveRequestBaselineAdjuster(List<double> Bounds, List<double> Rates) : IScheduledLeaveRequestBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledLeaveRequestBaseline item, double valueValue)
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

public sealed record MinimumScheduledLeaveRequestBaselineAdjuster(double Minimum) : IScheduledLeaveRequestBaselineAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledLeaveRequestBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScheduledLeaveRequestBaselineAdjuster(double Percent) : IScheduledLeaveRequestBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledLeaveRequestBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScheduledLeaveRequestBaselineAdjuster(double Cap) : IScheduledLeaveRequestBaselineAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledLeaveRequestBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScheduledLeaveRequestBaselineAdjuster(double Step) : IScheduledLeaveRequestBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledLeaveRequestBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScheduledLeaveRequestBaselineAdjuster(Registry Flags, string Flag, double Percent) : IScheduledLeaveRequestBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledLeaveRequestBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled leave request baseline steps in order and keeps a trace of each.</summary>
public sealed class ScheduledLeaveRequestBaselineEngine(List<IScheduledLeaveRequestBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledLeaveRequestBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledLeaveRequestBaselineEngine DefaultScheduledLeaveRequestBaselineEngine(Registry flags)
    {
        return new ScheduledLeaveRequestBaselineEngine(
            new List<IScheduledLeaveRequestBaselineAdjuster>
            {
                new TieredScheduledLeaveRequestBaselineAdjuster(
                    new List<double> { 6527.0, 7152.0 },
                    new List<double> { 0.9, 0.8 }),
                new MinimumScheduledLeaveRequestBaselineAdjuster(2.4),
                new PercentScheduledLeaveRequestBaselineAdjuster(-22.9),
                new CapScheduledLeaveRequestBaselineAdjuster(7941.0),
                new RoundingScheduledLeaveRequestBaselineAdjuster(0.5),
                new FlaggedScheduledLeaveRequestBaselineAdjuster(
                    flags,
                    "scheduled-leave-request-baseline-scheduled",
                    2.3),
            },
            new List<string>(),
            3.1,
            62178.5);
    }

    public double Evaluate(ScheduledLeaveRequestBaseline item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledLeaveRequestBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
