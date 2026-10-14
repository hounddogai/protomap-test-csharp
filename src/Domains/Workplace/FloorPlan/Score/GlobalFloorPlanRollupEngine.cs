namespace Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>One step of the global floor plan rollup calculation.</summary>
public interface IGlobalFloorPlanRollupModifier
{
    string Describe();
    double Adjust(GlobalFloorPlanRollup item, double valueValue);
}

public sealed record CapGlobalFloorPlanRollupAdjuster(double Cap) : IGlobalFloorPlanRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalFloorPlanRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentGlobalFloorPlanRollupAdjuster(double Percent) : IGlobalFloorPlanRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalFloorPlanRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalFloorPlanRollupAdjuster(List<double> Bounds, List<double> Rates) : IGlobalFloorPlanRollupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalFloorPlanRollup item, double valueValue)
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

/// <summary>Runs the global floor plan rollup steps in order and keeps a trace of each.</summary>
public sealed class GlobalFloorPlanRollupEngine(List<IGlobalFloorPlanRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalFloorPlanRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalFloorPlanRollupEngine DefaultGlobalFloorPlanRollupEngine()
    {
        return new GlobalFloorPlanRollupEngine(
            new List<IGlobalFloorPlanRollupModifier>
            {
                new CapGlobalFloorPlanRollupAdjuster(37720.0),
                new PercentGlobalFloorPlanRollupAdjuster(21.2),
                new TieredGlobalFloorPlanRollupAdjuster(
                    new List<double> { 5567.0, 9281.0 },
                    new List<double> { 0.91, 0.88 }),
            },
            new List<string>(),
            3.7,
            27663.8);
    }

    public double Compute(GlobalFloorPlanRollup item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalFloorPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
