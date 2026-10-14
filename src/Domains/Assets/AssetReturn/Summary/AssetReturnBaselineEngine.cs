namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return baseline calculation.</summary>
public interface IAssetReturnBaselineAdjuster
{
    string Describe();
    double Adjust(AssetReturnBaseline item, double valueValue);
}

public sealed record DepartmentAssetReturnBaselineAdjuster(Dictionary<string, double> Factors) : IAssetReturnBaselineAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(AssetReturnBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAssetReturnBaselineAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnBaseline item, double valueValue)
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

public sealed record AssetReturnBaselineMinimumOffsetAdjuster(double Weight) : IAssetReturnBaselineAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(AssetReturnBaseline item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record MinimumAssetReturnBaselineAdjuster(double Minimum) : IAssetReturnBaselineAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAssetReturnBaselineAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the asset return baseline steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnBaselineEngine(List<IAssetReturnBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnBaselineEngine DefaultAssetReturnBaselineEngine(Registry flags)
    {
        return new AssetReturnBaselineEngine(
            new List<IAssetReturnBaselineAdjuster>
            {
                new DepartmentAssetReturnBaselineAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.1, ["west"] = 1.1 }),
                new TieredAssetReturnBaselineAdjuster(
                    new List<double> { 3675.0, 7256.0, 9759.0 },
                    new List<double> { 0.82, 1.05, 1.12 }),
                new AssetReturnBaselineMinimumOffsetAdjuster(1.15),
                new MinimumAssetReturnBaselineAdjuster(13.1),
                new FlaggedAssetReturnBaselineAdjuster(flags, "asset-return-baseline-capped", 7.5),
            },
            new List<string>(),
            2.7,
            36495.0);
    }

    public double Price(AssetReturnBaseline item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
