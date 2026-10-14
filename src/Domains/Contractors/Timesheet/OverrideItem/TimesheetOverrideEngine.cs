namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet override calculation.</summary>
public interface ITimesheetOverrideFactor
{
    string Describe();
    double Adjust(TimesheetOverride item, double valueValue);
}

public sealed record MinimumTimesheetOverrideAdjuster(double Minimum) : ITimesheetOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTimesheetOverrideAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTimesheetOverrideAdjuster(double Step) : ITimesheetOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentTimesheetOverrideAdjuster(double Percent) : ITimesheetOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the timesheet override steps in order and keeps a trace of each.</summary>
public sealed class TimesheetOverrideEngine(List<ITimesheetOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetOverrideEngine DefaultTimesheetOverrideEngine(Registry flags)
    {
        return new TimesheetOverrideEngine(
            new List<ITimesheetOverrideFactor>
            {
                new MinimumTimesheetOverrideAdjuster(5.8),
                new FlaggedTimesheetOverrideAdjuster(flags, "timesheet-override-manual", 8.8),
                new RoundingTimesheetOverrideAdjuster(5.0),
                new PercentTimesheetOverrideAdjuster(-13.1),
            },
            new List<string>(),
            2.1,
            50169.4);
    }

    public double Compute(TimesheetOverride item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
