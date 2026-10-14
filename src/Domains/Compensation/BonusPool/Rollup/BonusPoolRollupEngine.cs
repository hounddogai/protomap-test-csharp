namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool rollup calculation.</summary>
public interface IBonusPoolRollupFactor
{
    string Describe();
    double Adjust(BonusPoolRollup item, double valueValue);
}

public sealed record TieredBonusPoolRollupAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolRollup item, double valueValue)
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

public sealed record BonusPoolRollupCeilingOffsetAdjuster(double Weight) : IBonusPoolRollupFactor
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(BonusPoolRollup item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record FlaggedBonusPoolRollupAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool rollup steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolRollupEngine(List<IBonusPoolRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolRollupEngine DefaultBonusPoolRollupEngine(Registry flags)
    {
        return new BonusPoolRollupEngine(
            new List<IBonusPoolRollupFactor>
            {
                new TieredBonusPoolRollupAdjuster(
                    new List<double> { 6619.0, 7330.0 },
                    new List<double> { 1.09, 0.83 }),
                new BonusPoolRollupCeilingOffsetAdjuster(1.41),
                new FlaggedBonusPoolRollupAdjuster(flags, "bonus-pool-rollup-default", 6.8),
            },
            new List<string>(),
            0.3,
            25887.1);
    }

    public double Compute(BonusPoolRollup item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
