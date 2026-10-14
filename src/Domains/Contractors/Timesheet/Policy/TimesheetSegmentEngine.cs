namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the timesheet segment calculation.</summary>
public interface ITimesheetSegmentStep
{
    string Describe();
    double Adjust(TimesheetSegment item, double valueValue);
}

public sealed record CapTimesheetSegmentAdjuster(double Cap) : ITimesheetSegmentStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTimesheetSegmentAdjuster(double Percent) : ITimesheetSegmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TimesheetSegmentRevisionOffsetAdjuster(double Weight) : ITimesheetSegmentStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(TimesheetSegment item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record FlaggedTimesheetSegmentAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetSegmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the timesheet segment steps in order and keeps a trace of each.</summary>
public sealed class TimesheetSegmentEngine(List<ITimesheetSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetSegmentEngine DefaultTimesheetSegmentEngine(Registry flags)
    {
        return new TimesheetSegmentEngine(
            new List<ITimesheetSegmentStep>
            {
                new CapTimesheetSegmentAdjuster(11829.0),
                new PercentTimesheetSegmentAdjuster(-10.9),
                new TimesheetSegmentRevisionOffsetAdjuster(0.89),
                new FlaggedTimesheetSegmentAdjuster(flags, "timesheet-segment-adaptive", 6.0),
            },
            new List<string>(),
            4.9,
            59051.4);
    }

    public double Evaluate(TimesheetSegment item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
