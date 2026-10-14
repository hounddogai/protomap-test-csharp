namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the timesheet rollup calculation.</summary>
public interface ITimesheetRollupFactor
{
    string Describe();
    double Adjust(TimesheetRollup item, double valueValue);
}

public sealed record DepartmentTimesheetRollupAdjuster(Dictionary<string, double> Factors) : ITimesheetRollupFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(TimesheetRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTimesheetRollupAdjuster(double Percent) : ITimesheetRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedTimesheetRollupAdjuster(Registry Flags, string Flag, double Percent) : ITimesheetRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TimesheetRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTimesheetRollupAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetRollup item, double valueValue)
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

/// <summary>Runs the timesheet rollup steps in order and keeps a trace of each.</summary>
public sealed class TimesheetRollupEngine(List<ITimesheetRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetRollupEngine DefaultTimesheetRollupEngine(Registry flags)
    {
        return new TimesheetRollupEngine(
            new List<ITimesheetRollupFactor>
            {
                new DepartmentTimesheetRollupAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.34, ["north"] = 1.38, ["online"] = 1.0 }),
                new PercentTimesheetRollupAdjuster(9.1),
                new FlaggedTimesheetRollupAdjuster(flags, "timesheet-rollup-default", -0.8),
                new TieredTimesheetRollupAdjuster(
                    new List<double> { 1826.0, 4621.0 },
                    new List<double> { 1.17, 0.83 }),
            },
            new List<string>(),
            2.3,
            42393.7);
    }

    public double Evaluate(TimesheetRollup item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
