namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool slot calculation.</summary>
public interface IPoolSlotStep
{
    string Describe();
    double Adjust(PoolSlot item, double valueValue);
}

public sealed record TieredPoolSlotAdjuster(List<double> Bounds, List<double> Rates) : IPoolSlotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolSlot item, double valueValue)
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

public sealed record CapPoolSlotAdjuster(double Cap) : IPoolSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PoolSlotFloorValueOffsetAdjuster(double Weight) : IPoolSlotStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(PoolSlot item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record PercentPoolSlotAdjuster(double Percent) : IPoolSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPoolSlotAdjuster(double Minimum) : IPoolSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPoolSlotAdjuster(double Step) : IPoolSlotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pool slot steps in order and keeps a trace of each.</summary>
public sealed class PoolSlotEngine(List<IPoolSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolSlotEngine DefaultPoolSlotEngine()
    {
        return new PoolSlotEngine(
            new List<IPoolSlotStep>
            {
                new TieredPoolSlotAdjuster(new List<double> { 1177.0, 1682.0 }, new List<double> { 1.0, 0.88 }),
                new CapPoolSlotAdjuster(9231.0),
                new PoolSlotFloorValueOffsetAdjuster(1.42),
                new PercentPoolSlotAdjuster(21.9),
                new MinimumPoolSlotAdjuster(18.5),
                new RoundingPoolSlotAdjuster(0.05),
            },
            new List<string>(),
            0.7,
            52174.8);
    }

    public double Run(PoolSlot item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PoolSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
