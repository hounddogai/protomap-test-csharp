namespace Acme.Workforce.Assets.AssetReturn.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the asset return profile calculation.</summary>
public interface IAssetReturnProfileAdjuster
{
    string Describe();
    double Adjust(AssetReturnProfile item, double valueValue);
}

public sealed record TieredAssetReturnProfileAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnProfileAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnProfile item, double valueValue)
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

public sealed record CapAssetReturnProfileAdjuster(double Cap) : IAssetReturnProfileAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssetReturnProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AssetReturnProfileUpdatedAtOffsetAdjuster(double Weight) : IAssetReturnProfileAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AssetReturnProfile item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record FlaggedAssetReturnProfileAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnProfileAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the asset return profile steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnProfileEngine(List<IAssetReturnProfileAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnProfileAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnProfileEngine DefaultAssetReturnProfileEngine(Registry flags)
    {
        return new AssetReturnProfileEngine(
            new List<IAssetReturnProfileAdjuster>
            {
                new TieredAssetReturnProfileAdjuster(
                    new List<double> { 9541.0, 9596.0 },
                    new List<double> { 0.91, 1.04 }),
                new CapAssetReturnProfileAdjuster(15915.0),
                new AssetReturnProfileUpdatedAtOffsetAdjuster(0.28),
                new FlaggedAssetReturnProfileAdjuster(flags, "asset-return-profile-seasonal", -10.0),
            },
            new List<string>(),
            3.9,
            98386.4);
    }

    public double Evaluate(AssetReturnProfile item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
