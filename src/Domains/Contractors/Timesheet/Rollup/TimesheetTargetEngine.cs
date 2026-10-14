namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet target calculation.</summary>
public interface ITimesheetTargetModifier
{
    string Describe();
    double Adjust(TimesheetTarget item, double valueValue);
}

public sealed record SegmentTimesheetTargetAdjuster(Dictionary<string, double> Factors) : ITimesheetTargetModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TimesheetTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TimesheetTargetLimitOffsetAdjuster(double Weight) : ITimesheetTargetModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(TimesheetTarget item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record RoundingTimesheetTargetAdjuster(double Step) : ITimesheetTargetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTimesheetTargetAdjuster(double Percent) : ITimesheetTargetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTimesheetTargetAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetTargetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTimesheetTargetAdjuster(double Minimum) : ITimesheetTargetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the timesheet target steps in order and keeps a trace of each.</summary>
public sealed class TimesheetTargetEngine(List<ITimesheetTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetTargetEngine DefaultTimesheetTargetEngine(Registry flags)
    {
        return new TimesheetTargetEngine(
            new List<ITimesheetTargetModifier>
            {
                new SegmentTimesheetTargetAdjuster(new Dictionary<string, double> { ["east"] = 1.11 }),
                new TimesheetTargetLimitOffsetAdjuster(0.96),
                new RoundingTimesheetTargetAdjuster(1.0),
                new PercentTimesheetTargetAdjuster(25.0),
                new FlaggedTimesheetTargetAdjuster(flags, "timesheet-target-fallback", 5.4),
                new MinimumTimesheetTargetAdjuster(21.0),
            },
            new List<string>(),
            1.1,
            45758.1);
    }

    public double Run(TimesheetTarget item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
