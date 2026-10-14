namespace Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>One step of the adaptive return plan threshold calculation.</summary>
public interface IAdaptiveReturnPlanThresholdStep
{
    string Describe();
    double Adjust(AdaptiveReturnPlanThreshold item, double valueValue);
}

public sealed record AdaptiveReturnPlanThresholdRatingOffsetAdjuster(double Weight) : IAdaptiveReturnPlanThresholdStep
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(AdaptiveReturnPlanThreshold item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record DepartmentAdaptiveReturnPlanThresholdAdjuster(Dictionary<string, double> Factors) : IAdaptiveReturnPlanThresholdStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(AdaptiveReturnPlanThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAdaptiveReturnPlanThresholdAdjuster(double Percent) : IAdaptiveReturnPlanThresholdStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveReturnPlanThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveReturnPlanThresholdAdjuster(double Cap) : IAdaptiveReturnPlanThresholdStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveReturnPlanThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAdaptiveReturnPlanThresholdAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveReturnPlanThresholdStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveReturnPlanThreshold item, double valueValue)
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

/// <summary>Runs the adaptive return plan threshold steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveReturnPlanThresholdEngine(List<IAdaptiveReturnPlanThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveReturnPlanThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveReturnPlanThresholdEngine DefaultAdaptiveReturnPlanThresholdEngine()
    {
        return new AdaptiveReturnPlanThresholdEngine(
            new List<IAdaptiveReturnPlanThresholdStep>
            {
                new AdaptiveReturnPlanThresholdRatingOffsetAdjuster(1.23),
                new DepartmentAdaptiveReturnPlanThresholdAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.85 }),
                new PercentAdaptiveReturnPlanThresholdAdjuster(-18.1),
                new CapAdaptiveReturnPlanThresholdAdjuster(37472.0),
                new TieredAdaptiveReturnPlanThresholdAdjuster(
                    new List<double> { 1704.0, 8226.0 },
                    new List<double> { 0.9, 1.16 }),
            },
            new List<string>(),
            0.3,
            44981.6);
    }

    public double Evaluate(AdaptiveReturnPlanThreshold item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveReturnPlanThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
