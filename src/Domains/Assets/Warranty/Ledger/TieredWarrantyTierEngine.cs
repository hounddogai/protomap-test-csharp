namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered warranty tier calculation.</summary>
public interface ITieredWarrantyTierFactor
{
    string Describe();
    double Adjust(TieredWarrantyTier item, double valueValue);
}

public sealed record FlaggedTieredWarrantyTierAdjuster(Registry Flags, string Flag, double Percent) : ITieredWarrantyTierFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredWarrantyTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelTieredWarrantyTierAdjuster(Dictionary<string, double> Factors) : ITieredWarrantyTierFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(TieredWarrantyTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentTieredWarrantyTierAdjuster(double Percent) : ITieredWarrantyTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredWarrantyTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredWarrantyTierAdjuster(double Step) : ITieredWarrantyTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredWarrantyTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTieredWarrantyTierAdjuster(List<double> Bounds, List<double> Rates) : ITieredWarrantyTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredWarrantyTier item, double valueValue)
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

public sealed record CapTieredWarrantyTierAdjuster(double Cap) : ITieredWarrantyTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredWarrantyTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tiered warranty tier steps in order and keeps a trace of each.</summary>
public sealed class TieredWarrantyTierEngine(List<ITieredWarrantyTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredWarrantyTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredWarrantyTierEngine DefaultTieredWarrantyTierEngine(Registry flags)
    {
        return new TieredWarrantyTierEngine(
            new List<ITieredWarrantyTierFactor>
            {
                new FlaggedTieredWarrantyTierAdjuster(flags, "tiered-warranty-tier-global", 7.0),
                new ChannelTieredWarrantyTierAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.92, ["west"] = 1.25, ["south"] = 0.9 }),
                new PercentTieredWarrantyTierAdjuster(18.5),
                new RoundingTieredWarrantyTierAdjuster(0.05),
                new TieredTieredWarrantyTierAdjuster(
                    new List<double> { 1922.0, 5380.0, 5465.0, 7382.0 },
                    new List<double> { 0.89, 1.05, 1.08, 0.91 }),
                new CapTieredWarrantyTierAdjuster(7343.0),
            },
            new List<string>(),
            1.2,
            71900.5);
    }

    public double Price(TieredWarrantyTier item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredWarrantyTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
