namespace Acme.Workforce.Talent.Pool.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool allocation calculation.</summary>
public interface IPoolAllocationAdjuster
{
    string Describe();
    double Adjust(PoolAllocation item, double valueValue);
}

public sealed record CapPoolAllocationAdjuster(double Cap) : IPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPoolAllocationAdjuster(double Percent) : IPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PoolAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PoolAllocationCreatedAtOffsetAdjuster(double Weight) : IPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(PoolAllocation item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record RoundingPoolAllocationAdjuster(double Step) : IPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPoolAllocationAdjuster(List<double> Bounds, List<double> Rates) : IPoolAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolAllocation item, double valueValue)
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

/// <summary>Runs the pool allocation steps in order and keeps a trace of each.</summary>
public sealed class PoolAllocationEngine(List<IPoolAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolAllocationEngine DefaultPoolAllocationEngine()
    {
        return new PoolAllocationEngine(
            new List<IPoolAllocationAdjuster>
            {
                new CapPoolAllocationAdjuster(46700.0),
                new PercentPoolAllocationAdjuster(-23.7),
                new PoolAllocationCreatedAtOffsetAdjuster(0.16),
                new RoundingPoolAllocationAdjuster(0.5),
                new TieredPoolAllocationAdjuster(
                    new List<double> { 5712.0, 5964.0 },
                    new List<double> { 0.96, 0.93 }),
            },
            new List<string>(),
            1.6,
            88497.6);
    }

    public double Run(PoolAllocation item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PoolAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
