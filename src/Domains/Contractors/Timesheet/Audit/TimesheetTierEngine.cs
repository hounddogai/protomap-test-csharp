namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet tier calculation.</summary>
public interface ITimesheetTierStep
{
    string Describe();
    double Adjust(TimesheetTier item, double valueValue);
}

public sealed record SourceTimesheetTierAdjuster(Dictionary<string, double> Factors) : ITimesheetTierStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(TimesheetTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTimesheetTierAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTimesheetTierAdjuster(double Percent) : ITimesheetTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTimesheetTierAdjuster(double Cap) : ITimesheetTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTimesheetTierAdjuster(double Step) : ITimesheetTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the timesheet tier steps in order and keeps a trace of each.</summary>
public sealed class TimesheetTierEngine(List<ITimesheetTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetTierEngine DefaultTimesheetTierEngine(Registry flags)
    {
        return new TimesheetTierEngine(
            new List<ITimesheetTierStep>
            {
                new SourceTimesheetTierAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.15, ["retail"] = 1.17, ["east"] = 1.4 }),
                new FlaggedTimesheetTierAdjuster(flags, "timesheet-tier-pending", 3.9),
                new PercentTimesheetTierAdjuster(2.5),
                new CapTimesheetTierAdjuster(49800.0),
                new RoundingTimesheetTierAdjuster(0.5),
            },
            new List<string>(),
            1.9,
            29831.4);
    }

    public double Run(TimesheetTier item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
