namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the bonus pool threshold calculation.</summary>
public interface IBonusPoolThresholdAdjuster
{
    string Describe();
    double Adjust(BonusPoolThreshold item, double valueValue);
}

public sealed record LabelBonusPoolThresholdAdjuster(Dictionary<string, double> Factors) : IBonusPoolThresholdAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BonusPoolThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BonusPoolThresholdDiscountOffsetAdjuster(double Weight) : IBonusPoolThresholdAdjuster
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(BonusPoolThreshold item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record TieredBonusPoolThresholdAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolThreshold item, double valueValue)
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

public sealed record PercentBonusPoolThresholdAdjuster(double Percent) : IBonusPoolThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBonusPoolThresholdAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the bonus pool threshold steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolThresholdEngine(List<IBonusPoolThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolThresholdEngine DefaultBonusPoolThresholdEngine(Registry flags)
    {
        return new BonusPoolThresholdEngine(
            new List<IBonusPoolThresholdAdjuster>
            {
                new LabelBonusPoolThresholdAdjuster(new Dictionary<string, double> { ["south"] = 0.72 }),
                new BonusPoolThresholdDiscountOffsetAdjuster(1.48),
                new TieredBonusPoolThresholdAdjuster(
                    new List<double> { 7260.0, 7555.0, 8469.0, 8648.0 },
                    new List<double> { 0.91, 0.81, 0.85, 1.02 }),
                new PercentBonusPoolThresholdAdjuster(-22.1),
                new FlaggedBonusPoolThresholdAdjuster(flags, "bonus-pool-threshold-seasonal", -6.1),
            },
            new List<string>(),
            2.7,
            92444.1);
    }

    public double Price(BonusPoolThreshold item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
