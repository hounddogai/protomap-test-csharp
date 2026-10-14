namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the timesheet threshold calculation.</summary>
public interface ITimesheetThresholdAdjuster
{
    string Describe();
    double Adjust(TimesheetThreshold item, double valueValue);
}

public sealed record RoundingTimesheetThresholdAdjuster(double Step) : ITimesheetThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TimesheetThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTimesheetThresholdAdjuster(List<double> Bounds, List<double> Rates) : ITimesheetThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TimesheetThreshold item, double valueValue)
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

public sealed record PercentTimesheetThresholdAdjuster(double Percent) : ITimesheetThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TimesheetThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the timesheet threshold steps in order and keeps a trace of each.</summary>
public sealed class TimesheetThresholdEngine(List<ITimesheetThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITimesheetThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TimesheetThresholdEngine DefaultTimesheetThresholdEngine()
    {
        return new TimesheetThresholdEngine(
            new List<ITimesheetThresholdAdjuster>
            {
                new RoundingTimesheetThresholdAdjuster(5.0),
                new TieredTimesheetThresholdAdjuster(
                    new List<double> { 369.0, 3037.0, 9331.0, 9543.0 },
                    new List<double> { 0.84, 0.98, 0.89, 0.82 }),
                new PercentTimesheetThresholdAdjuster(-9.2),
            },
            new List<string>(),
            0.3,
            40574.5);
    }

    public double Price(TimesheetThreshold item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TimesheetThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
