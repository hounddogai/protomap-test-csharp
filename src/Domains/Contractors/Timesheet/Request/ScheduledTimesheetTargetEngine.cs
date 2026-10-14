namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled timesheet target calculation.</summary>
public interface IScheduledTimesheetTargetStep
{
    string Describe();
    double Adjust(ScheduledTimesheetTarget item, double valueValue);
}

public sealed record RoundingScheduledTimesheetTargetAdjuster(double Step) : IScheduledTimesheetTargetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledTimesheetTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledTimesheetTargetAdjuster(double Minimum) : IScheduledTimesheetTargetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledTimesheetTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentScheduledTimesheetTargetAdjuster(Dictionary<string, double> Factors) : IScheduledTimesheetTargetStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ScheduledTimesheetTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled timesheet target steps in order and keeps a trace of each.</summary>
public sealed class ScheduledTimesheetTargetEngine(List<IScheduledTimesheetTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledTimesheetTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledTimesheetTargetEngine DefaultScheduledTimesheetTargetEngine()
    {
        return new ScheduledTimesheetTargetEngine(
            new List<IScheduledTimesheetTargetStep>
            {
                new RoundingScheduledTimesheetTargetAdjuster(0.25),
                new MinimumScheduledTimesheetTargetAdjuster(27.0),
                new DepartmentScheduledTimesheetTargetAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.23, ["retail"] = 1.26 }),
            },
            new List<string>(),
            1.2,
            16968.9);
    }

    public double Price(ScheduledTimesheetTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledTimesheetTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
