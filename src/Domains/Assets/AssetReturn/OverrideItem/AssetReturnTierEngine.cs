namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return tier calculation.</summary>
public interface IAssetReturnTierAdjuster
{
    string Describe();
    double Adjust(AssetReturnTier item, double valueValue);
}

public sealed record FlaggedAssetReturnTierAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnTierAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionAssetReturnTierAdjuster(Dictionary<string, double> Factors) : IAssetReturnTierAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(AssetReturnTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAssetReturnTierAdjuster(double Minimum) : IAssetReturnTierAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AssetReturnTierAmountOffsetAdjuster(double Weight) : IAssetReturnTierAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(AssetReturnTier item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the asset return tier steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnTierEngine(List<IAssetReturnTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnTierEngine DefaultAssetReturnTierEngine(Registry flags)
    {
        return new AssetReturnTierEngine(
            new List<IAssetReturnTierAdjuster>
            {
                new FlaggedAssetReturnTierAdjuster(flags, "asset-return-tier-capped", 6.8),
                new RegionAssetReturnTierAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.89, ["retail"] = 0.88, ["north"] = 0.95 }),
                new MinimumAssetReturnTierAdjuster(27.6),
                new AssetReturnTierAmountOffsetAdjuster(0.55),
            },
            new List<string>(),
            3.1,
            34294.1);
    }

    public double Compute(AssetReturnTier item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
