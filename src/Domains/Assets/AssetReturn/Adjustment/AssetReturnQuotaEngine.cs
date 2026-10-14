namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

/// <summary>One step of the asset return quota calculation.</summary>
public interface IAssetReturnQuotaModifier
{
    string Describe();
    double Adjust(AssetReturnQuota item, double valueValue);
}

public sealed record TieredAssetReturnQuotaAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnQuota item, double valueValue)
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

public sealed record PercentAssetReturnQuotaAdjuster(double Percent) : IAssetReturnQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameAssetReturnQuotaAdjuster(Dictionary<string, double> Factors) : IAssetReturnQuotaModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AssetReturnQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the asset return quota steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnQuotaEngine(List<IAssetReturnQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnQuotaEngine DefaultAssetReturnQuotaEngine()
    {
        return new AssetReturnQuotaEngine(
            new List<IAssetReturnQuotaModifier>
            {
                new TieredAssetReturnQuotaAdjuster(
                    new List<double> { 1248.0, 3326.0, 5016.0 },
                    new List<double> { 0.96, 1.13, 1.16 }),
                new PercentAssetReturnQuotaAdjuster(-11.4),
                new NameAssetReturnQuotaAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.8, ["south"] = 1.15 }),
            },
            new List<string>(),
            4.3,
            16728.0);
    }

    public double Run(AssetReturnQuota item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
