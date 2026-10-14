namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

/// <summary>One step of the weighted floor plan entry calculation.</summary>
public interface IWeightedFloorPlanEntryModifier
{
    string Describe();
    double Adjust(WeightedFloorPlanEntry item, double valueValue);
}

public sealed record CapWeightedFloorPlanEntryAdjuster(double Cap) : IWeightedFloorPlanEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedFloorPlanEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedFloorPlanEntryAdjuster(double Percent) : IWeightedFloorPlanEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedFloorPlanEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitWeightedFloorPlanEntryAdjuster(Dictionary<string, double> Factors) : IWeightedFloorPlanEntryModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(WeightedFloorPlanEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted floor plan entry steps in order and keeps a trace of each.</summary>
public sealed class WeightedFloorPlanEntryEngine(List<IWeightedFloorPlanEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedFloorPlanEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedFloorPlanEntryEngine DefaultWeightedFloorPlanEntryEngine()
    {
        return new WeightedFloorPlanEntryEngine(
            new List<IWeightedFloorPlanEntryModifier>
            {
                new CapWeightedFloorPlanEntryAdjuster(797.0),
                new PercentWeightedFloorPlanEntryAdjuster(-10.2),
                new UnitWeightedFloorPlanEntryAdjuster(new Dictionary<string, double> { ["central"] = 1.24 }),
            },
            new List<string>(),
            1.6,
            4063.9);
    }

    public double Compute(WeightedFloorPlanEntry item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedFloorPlanEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
