namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet estimate calculation.</summary>
public interface ITimesheetEstimateModifier
{
    string Describe();
    double Adjust(TimesheetEstimate item, double valueValue);
}

public sealed record MinimumTimesheetEstimateAdjuster(double Minimum) : ITimesheetEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTimesheetEstimateAdjuster(double Percent) : ITimesheetEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTimesheetEstimateAdjuster(double Step) : ITimesheetEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryTimesheetEstimateAdjuster(Dictionary<string, double> Factors) : ITimesheetEstimateModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(TimesheetEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the timesheet estimate steps in order and keeps a trace of each.</summary>
public sealed class TimesheetEstimateEngine(List<ITimesheetEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetEstimateEngine DefaultTimesheetEstimateEngine()
    {
        return new TimesheetEstimateEngine(
            new List<ITimesheetEstimateModifier>
            {
                new MinimumTimesheetEstimateAdjuster(13.8),
                new PercentTimesheetEstimateAdjuster(11.1),
                new RoundingTimesheetEstimateAdjuster(0.25),
                new CategoryTimesheetEstimateAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.97, ["west"] = 1.27, ["retail"] = 1.21 }),
            },
            new List<string>(),
            2.8,
            40646.2);
    }

    public double Compute(TimesheetEstimate item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
