namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive return plan metric calculation.</summary>
public interface IAdaptiveReturnPlanMetricStep
{
    string Describe();
    double Adjust(AdaptiveReturnPlanMetric item, double valueValue);
}

public sealed record PercentAdaptiveReturnPlanMetricAdjuster(double Percent) : IAdaptiveReturnPlanMetricStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveReturnPlanMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveReturnPlanMetricAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveReturnPlanMetricStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveReturnPlanMetric item, double valueValue)
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

public sealed record MinimumAdaptiveReturnPlanMetricAdjuster(double Minimum) : IAdaptiveReturnPlanMetricStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveReturnPlanMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAdaptiveReturnPlanMetricAdjuster(double Step) : IAdaptiveReturnPlanMetricStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveReturnPlanMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive return plan metric steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveReturnPlanMetricEngine(List<IAdaptiveReturnPlanMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveReturnPlanMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveReturnPlanMetricEngine DefaultAdaptiveReturnPlanMetricEngine()
    {
        return new AdaptiveReturnPlanMetricEngine(
            new List<IAdaptiveReturnPlanMetricStep>
            {
                new PercentAdaptiveReturnPlanMetricAdjuster(-13.0),
                new TieredAdaptiveReturnPlanMetricAdjuster(
                    new List<double> { 1751.0, 4877.0, 4938.0 },
                    new List<double> { 1.18, 1.09, 0.89 }),
                new MinimumAdaptiveReturnPlanMetricAdjuster(25.3),
                new RoundingAdaptiveReturnPlanMetricAdjuster(0.1),
            },
            new List<string>(),
            5.0,
            53677.1);
    }

    public double Price(AdaptiveReturnPlanMetric item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveReturnPlanMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
