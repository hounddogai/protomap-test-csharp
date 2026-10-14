namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet exception calculation.</summary>
public interface ITimesheetExceptionModifier
{
    string Describe();
    double Adjust(TimesheetException item, double valueValue);
}

public sealed record TeamTimesheetExceptionAdjuster(Dictionary<string, double> Factors) : ITimesheetExceptionModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(TimesheetException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTimesheetExceptionAdjuster(double Step) : ITimesheetExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapTimesheetExceptionAdjuster(double Cap) : ITimesheetExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the timesheet exception steps in order and keeps a trace of each.</summary>
public sealed class TimesheetExceptionEngine(List<ITimesheetExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetExceptionEngine DefaultTimesheetExceptionEngine()
    {
        return new TimesheetExceptionEngine(
            new List<ITimesheetExceptionModifier>
            {
                new TeamTimesheetExceptionAdjuster(new Dictionary<string, double> { ["central"] = 0.98 }),
                new RoundingTimesheetExceptionAdjuster(0.5),
                new CapTimesheetExceptionAdjuster(19746.0),
            },
            new List<string>(),
            2.3,
            1089.1);
    }

    public double Price(TimesheetException item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
