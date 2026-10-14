namespace Acme.Workforce.Assets.AssetReturn.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return estimate calculation.</summary>
public interface IAssetReturnEstimateAdjuster
{
    string Describe();
    double Adjust(AssetReturnEstimate item, double valueValue);
}

public sealed record ChannelAssetReturnEstimateAdjuster(Dictionary<string, double> Factors) : IAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(AssetReturnEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAssetReturnEstimateAdjuster(double Cap) : IAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssetReturnEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAssetReturnEstimateAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnEstimate item, double valueValue)
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

public sealed record MinimumAssetReturnEstimateAdjuster(double Minimum) : IAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAssetReturnEstimateAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the asset return estimate steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnEstimateEngine(List<IAssetReturnEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnEstimateEngine DefaultAssetReturnEstimateEngine(Registry flags)
    {
        return new AssetReturnEstimateEngine(
            new List<IAssetReturnEstimateAdjuster>
            {
                new ChannelAssetReturnEstimateAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.33, ["north"] = 0.9 }),
                new CapAssetReturnEstimateAdjuster(42633.0),
                new TieredAssetReturnEstimateAdjuster(
                    new List<double> { 2662.0, 4271.0 },
                    new List<double> { 1.1, 1.13 }),
                new MinimumAssetReturnEstimateAdjuster(42.5),
                new FlaggedAssetReturnEstimateAdjuster(flags, "asset-return-estimate-primary", -7.2),
            },
            new List<string>(),
            2.3,
            92055.9);
    }

    public double Run(AssetReturnEstimate item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
