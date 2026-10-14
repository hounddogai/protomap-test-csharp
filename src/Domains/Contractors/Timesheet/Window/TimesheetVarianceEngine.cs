namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet variance calculation.</summary>
public interface ITimesheetVarianceModifier
{
    string Describe();
    double Adjust(TimesheetVariance item, double valueValue);
}

public sealed record LabelTimesheetVarianceAdjuster(Dictionary<string, double> Factors) : ITimesheetVarianceModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(TimesheetVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumTimesheetVarianceAdjuster(double Minimum) : ITimesheetVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingTimesheetVarianceAdjuster(double Step) : ITimesheetVarianceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the timesheet variance steps in order and keeps a trace of each.</summary>
public sealed class TimesheetVarianceEngine(List<ITimesheetVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetVarianceEngine DefaultTimesheetVarianceEngine()
    {
        return new TimesheetVarianceEngine(
            new List<ITimesheetVarianceModifier>
            {
                new LabelTimesheetVarianceAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.95, ["north"] = 1.24 }),
                new MinimumTimesheetVarianceAdjuster(47.0),
                new RoundingTimesheetVarianceAdjuster(0.5),
            },
            new List<string>(),
            0.3,
            2381.9);
    }

    public double Run(TimesheetVariance item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
