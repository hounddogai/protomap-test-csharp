namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet milestone calculation.</summary>
public interface ITimesheetMilestoneStep
{
    string Describe();
    double Adjust(TimesheetMilestone item, double valueValue);
}

public sealed record CapTimesheetMilestoneAdjuster(double Cap) : ITimesheetMilestoneStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTimesheetMilestoneAdjuster(double Step) : ITimesheetMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTimesheetMilestoneAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetMilestoneStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetMilestone item, double valueValue)
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

public sealed record MinimumTimesheetMilestoneAdjuster(double Minimum) : ITimesheetMilestoneStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TimesheetMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameTimesheetMilestoneAdjuster(Dictionary<string, double> Factors) : ITimesheetMilestoneStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TimesheetMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the timesheet milestone steps in order and keeps a trace of each.</summary>
public sealed class TimesheetMilestoneEngine(List<ITimesheetMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetMilestoneEngine DefaultTimesheetMilestoneEngine()
    {
        return new TimesheetMilestoneEngine(
            new List<ITimesheetMilestoneStep>
            {
                new CapTimesheetMilestoneAdjuster(6299.0),
                new RoundingTimesheetMilestoneAdjuster(0.25),
                new TieredTimesheetMilestoneAdjuster(
                    new List<double> { 4214.0, 5593.0 },
                    new List<double> { 0.83, 1.16 }),
                new MinimumTimesheetMilestoneAdjuster(35.3),
                new NameTimesheetMilestoneAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.32, ["online"] = 0.93, ["south"] = 1.35 }),
            },
            new List<string>(),
            1.7,
            40154.5);
    }

    public double Evaluate(TimesheetMilestone item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
