namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted floor plan metric calculation.</summary>
public interface IWeightedFloorPlanMetricAdjuster
{
    string Describe();
    double Adjust(WeightedFloorPlanMetric item, double valueValue);
}

public sealed record TieredWeightedFloorPlanMetricAdjuster(List<double> Bounds, List<double> Rates) : IWeightedFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedFloorPlanMetric item, double valueValue)
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

public sealed record FlaggedWeightedFloorPlanMetricAdjuster(Registry Flags, string Flag, double Percent) : IWeightedFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedFloorPlanMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedFloorPlanMetricAdjuster(double Minimum) : IWeightedFloorPlanMetricAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedFloorPlanMetric item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted floor plan metric steps in order and keeps a trace of each.</summary>
public sealed class WeightedFloorPlanMetricEngine(List<IWeightedFloorPlanMetricAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedFloorPlanMetricAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedFloorPlanMetricEngine DefaultWeightedFloorPlanMetricEngine(Registry flags)
    {
        return new WeightedFloorPlanMetricEngine(
            new List<IWeightedFloorPlanMetricAdjuster>
            {
                new TieredWeightedFloorPlanMetricAdjuster(
                    new List<double> { 268.0, 4651.0, 4674.0 },
                    new List<double> { 1.18, 1.1, 0.97 }),
                new FlaggedWeightedFloorPlanMetricAdjuster(flags, "weighted-floor-plan-metric-tiered", -3.2),
                new MinimumWeightedFloorPlanMetricAdjuster(40.7),
            },
            new List<string>(),
            4.1,
            94900.2);
    }

    public double Compute(WeightedFloorPlanMetric item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedFloorPlanMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
