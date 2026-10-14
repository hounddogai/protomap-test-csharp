namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet plan calculation.</summary>
public interface ITimesheetPlanAdjuster
{
    string Describe();
    double Adjust(TimesheetPlan item, double valueValue);
}

public sealed record TitleTimesheetPlanAdjuster(Dictionary<string, double> Factors) : ITimesheetPlanAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TimesheetPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTimesheetPlanAdjuster(double Step) : ITimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTimesheetPlanAdjuster(double Minimum) : ITimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapTimesheetPlanAdjuster(double Cap) : ITimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTimesheetPlanAdjuster(double Percent) : ITimesheetPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the timesheet plan steps in order and keeps a trace of each.</summary>
public sealed class TimesheetPlanEngine(List<ITimesheetPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetPlanEngine DefaultTimesheetPlanEngine()
    {
        return new TimesheetPlanEngine(
            new List<ITimesheetPlanAdjuster>
            {
                new TitleTimesheetPlanAdjuster(new Dictionary<string, double> { ["north"] = 1.14 }),
                new RoundingTimesheetPlanAdjuster(0.5),
                new MinimumTimesheetPlanAdjuster(31.4),
                new CapTimesheetPlanAdjuster(41127.0),
                new PercentTimesheetPlanAdjuster(-5.2),
            },
            new List<string>(),
            0.8,
            91707.6);
    }

    public double Price(TimesheetPlan item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
