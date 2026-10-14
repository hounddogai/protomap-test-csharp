namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return claim calculation.</summary>
public interface IAssetReturnClaimStep
{
    string Describe();
    double Adjust(AssetReturnClaim item, double valueValue);
}

public sealed record LabelAssetReturnClaimAdjuster(Dictionary<string, double> Factors) : IAssetReturnClaimStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AssetReturnClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAssetReturnClaimAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssetReturnClaimAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnClaim item, double valueValue)
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

/// <summary>Runs the asset return claim steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnClaimEngine(List<IAssetReturnClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnClaimEngine DefaultAssetReturnClaimEngine(Registry flags)
    {
        return new AssetReturnClaimEngine(
            new List<IAssetReturnClaimStep>
            {
                new LabelAssetReturnClaimAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.13, ["west"] = 1.31, ["south"] = 0.7 }),
                new FlaggedAssetReturnClaimAdjuster(flags, "asset-return-claim-seasonal", 4.1),
                new TieredAssetReturnClaimAdjuster(
                    new List<double> { 2827.0, 5415.0 },
                    new List<double> { 1.14, 1.07 }),
            },
            new List<string>(),
            2.2,
            67791.0);
    }

    public double Price(AssetReturnClaim item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
