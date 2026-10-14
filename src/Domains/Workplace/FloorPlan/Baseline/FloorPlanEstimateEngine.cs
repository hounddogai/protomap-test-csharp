namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan estimate calculation.</summary>
public interface IFloorPlanEstimateAdjuster
{
    string Describe();
    double Adjust(FloorPlanEstimate item, double valueValue);
}

public sealed record RoundingFloorPlanEstimateAdjuster(double Step) : IFloorPlanEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FloorPlanEstimateVersionOffsetAdjuster(double Weight) : IFloorPlanEstimateAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(FloorPlanEstimate item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record TieredFloorPlanEstimateAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanEstimateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanEstimate item, double valueValue)
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

/// <summary>Runs the floor plan estimate steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanEstimateEngine(List<IFloorPlanEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanEstimateEngine DefaultFloorPlanEstimateEngine()
    {
        return new FloorPlanEstimateEngine(
            new List<IFloorPlanEstimateAdjuster>
            {
                new RoundingFloorPlanEstimateAdjuster(0.5),
                new FloorPlanEstimateVersionOffsetAdjuster(1.35),
                new TieredFloorPlanEstimateAdjuster(
                    new List<double> { 958.0, 2199.0 },
                    new List<double> { 1.15, 0.91 }),
            },
            new List<string>(),
            4.5,
            5218.0);
    }

    public double Price(FloorPlanEstimate item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
