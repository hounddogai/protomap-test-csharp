namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the asset return variance calculation.</summary>
public interface IAssetReturnVarianceAdjuster
{
    string Describe();
    double Adjust(AssetReturnVariance item, double valueValue);
}

public sealed record CapAssetReturnVarianceAdjuster(double Cap) : IAssetReturnVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssetReturnVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAssetReturnVarianceAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnVariance item, double valueValue)
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

public sealed record PercentAssetReturnVarianceAdjuster(double Percent) : IAssetReturnVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AssetReturnVarianceSequenceOffsetAdjuster(double Weight) : IAssetReturnVarianceAdjuster
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(AssetReturnVariance item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record FlaggedAssetReturnVarianceAdjuster(Registry Flags, string Flag, double Percent) : IAssetReturnVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AssetReturnVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the asset return variance steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnVarianceEngine(List<IAssetReturnVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnVarianceEngine DefaultAssetReturnVarianceEngine(Registry flags)
    {
        return new AssetReturnVarianceEngine(
            new List<IAssetReturnVarianceAdjuster>
            {
                new CapAssetReturnVarianceAdjuster(44533.0),
                new TieredAssetReturnVarianceAdjuster(
                    new List<double> { 1079.0, 7250.0, 8064.0, 9332.0 },
                    new List<double> { 1.04, 1.07, 0.92, 1.18 }),
                new PercentAssetReturnVarianceAdjuster(16.3),
                new AssetReturnVarianceSequenceOffsetAdjuster(0.39),
                new FlaggedAssetReturnVarianceAdjuster(flags, "asset-return-variance-scheduled", -1.7),
            },
            new List<string>(),
            3.7,
            25690.8);
    }

    public double Price(AssetReturnVariance item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
