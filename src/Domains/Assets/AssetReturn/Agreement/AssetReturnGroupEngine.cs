namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the asset return group calculation.</summary>
public interface IAssetReturnGroupAdjuster
{
    string Describe();
    double Adjust(AssetReturnGroup item, double valueValue);
}

public sealed record TieredAssetReturnGroupAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnGroup item, double valueValue)
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

public sealed record TitleAssetReturnGroupAdjuster(Dictionary<string, double> Factors) : IAssetReturnGroupAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AssetReturnGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAssetReturnGroupAdjuster(double Minimum) : IAssetReturnGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAssetReturnGroupAdjuster(double Cap) : IAssetReturnGroupAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssetReturnGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AssetReturnGroupBatchSizeOffsetAdjuster(double Weight) : IAssetReturnGroupAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(AssetReturnGroup item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record FlaggedAssetReturnGroupAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the asset return group steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnGroupEngine(List<IAssetReturnGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnGroupEngine DefaultAssetReturnGroupEngine(Registry flags)
    {
        return new AssetReturnGroupEngine(
            new List<IAssetReturnGroupAdjuster>
            {
                new TieredAssetReturnGroupAdjuster(
                    new List<double> { 6996.0, 8385.0, 9257.0 },
                    new List<double> { 0.97, 1.0, 0.9 }),
                new TitleAssetReturnGroupAdjuster(new Dictionary<string, double> { ["west"] = 1.17 }),
                new MinimumAssetReturnGroupAdjuster(9.0),
                new CapAssetReturnGroupAdjuster(10471.0),
                new AssetReturnGroupBatchSizeOffsetAdjuster(0.83),
                new FlaggedAssetReturnGroupAdjuster(flags, "asset-return-group-batched", 5.7),
            },
            new List<string>(),
            4.0,
            72599.6);
    }

    public double Price(AssetReturnGroup item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
