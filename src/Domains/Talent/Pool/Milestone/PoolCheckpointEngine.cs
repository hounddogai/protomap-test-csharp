namespace Acme.Workforce.Talent.Pool.Milestone;

/// <summary>One step of the pool checkpoint calculation.</summary>
public interface IPoolCheckpointFactor
{
    string Describe();
    double Adjust(PoolCheckpoint item, double valueValue);
}

public sealed record PoolCheckpointUpdatedAtOffsetAdjuster(double Weight) : IPoolCheckpointFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(PoolCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record TieredPoolCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IPoolCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolCheckpoint item, double valueValue)
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

public sealed record CapPoolCheckpointAdjuster(double Cap) : IPoolCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NamePoolCheckpointAdjuster(Dictionary<string, double> Factors) : IPoolCheckpointFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PoolCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pool checkpoint steps in order and keeps a trace of each.</summary>
public sealed class PoolCheckpointEngine(List<IPoolCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolCheckpointEngine DefaultPoolCheckpointEngine()
    {
        return new PoolCheckpointEngine(
            new List<IPoolCheckpointFactor>
            {
                new PoolCheckpointUpdatedAtOffsetAdjuster(0.67),
                new TieredPoolCheckpointAdjuster(
                    new List<double> { 277.0, 6578.0, 9277.0 },
                    new List<double> { 0.86, 1.08, 1.06 }),
                new CapPoolCheckpointAdjuster(16928.0),
                new NamePoolCheckpointAdjuster(new Dictionary<string, double> { ["north"] = 0.72, ["west"] = 1.17 }),
            },
            new List<string>(),
            0.5,
            31613.4);
    }

    public double Price(PoolCheckpoint item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PoolCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
