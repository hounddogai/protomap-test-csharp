namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet group calculation.</summary>
public interface ITimesheetGroupStep
{
    string Describe();
    double Adjust(TimesheetGroup item, double valueValue);
}

public sealed record PercentTimesheetGroupAdjuster(double Percent) : ITimesheetGroupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleTimesheetGroupAdjuster(Dictionary<string, double> Factors) : ITimesheetGroupStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TimesheetGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TimesheetGroupDiscountOffsetAdjuster(double Weight) : ITimesheetGroupStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(TimesheetGroup item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapTimesheetGroupAdjuster(double Cap) : ITimesheetGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TimesheetGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredTimesheetGroupAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetGroup item, double valueValue)
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

/// <summary>Runs the timesheet group steps in order and keeps a trace of each.</summary>
public sealed class TimesheetGroupEngine(List<ITimesheetGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetGroupEngine DefaultTimesheetGroupEngine()
    {
        return new TimesheetGroupEngine(
            new List<ITimesheetGroupStep>
            {
                new PercentTimesheetGroupAdjuster(-2.8),
                new TitleTimesheetGroupAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.92, ["online"] = 0.71, ["south"] = 1.19 }),
                new TimesheetGroupDiscountOffsetAdjuster(1.79),
                new CapTimesheetGroupAdjuster(8653.0),
                new TieredTimesheetGroupAdjuster(
                    new List<double> { 3577.0, 9069.0 },
                    new List<double> { 1.17, 0.82 }),
            },
            new List<string>(),
            4.9,
            42302.5);
    }

    public double Run(TimesheetGroup item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
