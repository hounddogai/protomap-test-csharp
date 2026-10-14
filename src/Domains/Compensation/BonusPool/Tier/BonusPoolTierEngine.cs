namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the bonus pool tier calculation.</summary>
public interface IBonusPoolTierStep
{
    string Describe();
    double Adjust(BonusPoolTier item, double valueValue);
}

public sealed record TieredBonusPoolTierAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolTier item, double valueValue)
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

public sealed record FlaggedBonusPoolTierAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBonusPoolTierAdjuster(double Percent) : IBonusPoolTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool tier steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolTierEngine(List<IBonusPoolTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolTierEngine DefaultBonusPoolTierEngine(Registry flags)
    {
        return new BonusPoolTierEngine(
            new List<IBonusPoolTierStep>
            {
                new TieredBonusPoolTierAdjuster(
                    new List<double> { 1145.0, 2658.0, 9449.0 },
                    new List<double> { 0.85, 0.98, 0.96 }),
                new FlaggedBonusPoolTierAdjuster(flags, "bonus-pool-tier-weighted", -5.5),
                new PercentBonusPoolTierAdjuster(6.9),
            },
            new List<string>(),
            2.4,
            48092.8);
    }

    public double Compute(BonusPoolTier item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
