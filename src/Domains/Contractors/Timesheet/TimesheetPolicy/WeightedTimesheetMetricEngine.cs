namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted timesheet metric calculation.</summary>
public interface IWeightedTimesheetMetricAdjuster
{
    string Describe();
    double Adjust(WeightedTimesheetMetric item, double valueValue);
}

public sealed record PercentWeightedTimesheetMetricAdjuster(double Percent) : IWeightedTimesheetMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedTimesheetMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedTimesheetMetricAdjuster(List<double> Bounds, List<double> Rates) : IWeightedTimesheetMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedTimesheetMetric item, double valueValue)
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

public sealed record MinimumWeightedTimesheetMetricAdjuster(double Minimum) : IWeightedTimesheetMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedTimesheetMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted timesheet metric steps in order and keeps a trace of each.</summary>
public sealed class WeightedTimesheetMetricEngine(List<IWeightedTimesheetMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedTimesheetMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedTimesheetMetricEngine DefaultWeightedTimesheetMetricEngine()
    {
        return new WeightedTimesheetMetricEngine(
            new List<IWeightedTimesheetMetricAdjuster>
            {
                new PercentWeightedTimesheetMetricAdjuster(17.0),
                new TieredWeightedTimesheetMetricAdjuster(
                    new List<double> { 2953.0, 2989.0 },
                    new List<double> { 1.04, 0.99 }),
                new MinimumWeightedTimesheetMetricAdjuster(17.3),
            },
            new List<string>(),
            0.2,
            45518.4);
    }

    public double Price(WeightedTimesheetMetric item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedTimesheetMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
