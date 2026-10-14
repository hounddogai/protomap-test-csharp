namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool rollup calculation.</summary>
public interface IPoolRollupAdjuster
{
    string Describe();
    double Adjust(PoolRollup item, double valueValue);
}

public sealed record FlaggedPoolRollupAdjuster(Registry Flags, string Flag, double Percent) : IPoolRollupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPoolRollupAdjuster(List<double> Bounds, List<double> Rates) : IPoolRollupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PoolRollup item, double valueValue)
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

public sealed record MinimumPoolRollupAdjuster(double Minimum) : IPoolRollupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pool rollup steps in order and keeps a trace of each.</summary>
public sealed class PoolRollupEngine(List<IPoolRollupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolRollupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolRollupEngine DefaultPoolRollupEngine(Registry flags)
    {
        return new PoolRollupEngine(
            new List<IPoolRollupAdjuster>
            {
                new FlaggedPoolRollupAdjuster(flags, "pool-rollup-capped", 4.1),
                new TieredPoolRollupAdjuster(
                    new List<double> { 82.0, 592.0, 2420.0, 9095.0 },
                    new List<double> { 1.01, 0.97, 0.88, 1.11 }),
                new MinimumPoolRollupAdjuster(31.2),
            },
            new List<string>(),
            1.0,
            28201.8);
    }

    public double Compute(PoolRollup item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PoolRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
