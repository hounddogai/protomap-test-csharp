namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>One step of the seasonal floor plan batch calculation.</summary>
public interface ISeasonalFloorPlanBatchModifier
{
    string Describe();
    double Adjust(SeasonalFloorPlanBatch item, double valueValue);
}

public sealed record TieredSeasonalFloorPlanBatchAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalFloorPlanBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalFloorPlanBatch item, double valueValue)
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

public sealed record SeasonalFloorPlanBatchFloorValueOffsetAdjuster(double Weight) : ISeasonalFloorPlanBatchModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(SeasonalFloorPlanBatch item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record NameSeasonalFloorPlanBatchAdjuster(Dictionary<string, double> Factors) : ISeasonalFloorPlanBatchModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SeasonalFloorPlanBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSeasonalFloorPlanBatchAdjuster(double Cap) : ISeasonalFloorPlanBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalFloorPlanBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalFloorPlanBatchAdjuster(double Minimum) : ISeasonalFloorPlanBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalFloorPlanBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal floor plan batch steps in order and keeps a trace of each.</summary>
public sealed class SeasonalFloorPlanBatchEngine(List<ISeasonalFloorPlanBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalFloorPlanBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalFloorPlanBatchEngine DefaultSeasonalFloorPlanBatchEngine()
    {
        return new SeasonalFloorPlanBatchEngine(
            new List<ISeasonalFloorPlanBatchModifier>
            {
                new TieredSeasonalFloorPlanBatchAdjuster(
                    new List<double> { 176.0, 7681.0, 8206.0 },
                    new List<double> { 1.13, 1.15, 1.12 }),
                new SeasonalFloorPlanBatchFloorValueOffsetAdjuster(1.87),
                new NameSeasonalFloorPlanBatchAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.19, ["south"] = 0.81 }),
                new CapSeasonalFloorPlanBatchAdjuster(36789.0),
                new MinimumSeasonalFloorPlanBatchAdjuster(35.4),
            },
            new List<string>(),
            2.5,
            66746.2);
    }

    public double Compute(SeasonalFloorPlanBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalFloorPlanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
