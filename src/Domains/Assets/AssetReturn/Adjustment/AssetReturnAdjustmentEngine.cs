namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return adjustment calculation.</summary>
public interface IAssetReturnAdjustmentAdjuster
{
    string Describe();
    double Adjust(AssetReturnAdjustment item, double valueValue);
}

public sealed record RoundingAssetReturnAdjustmentAdjuster(double Step) : IAssetReturnAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitAssetReturnAdjustmentAdjuster(Dictionary<string, double> Factors) : IAssetReturnAdjustmentAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AssetReturnAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAssetReturnAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnAdjustment item, double valueValue)
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

/// <summary>Runs the asset return adjustment steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnAdjustmentEngine(List<IAssetReturnAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnAdjustmentEngine DefaultAssetReturnAdjustmentEngine()
    {
        return new AssetReturnAdjustmentEngine(
            new List<IAssetReturnAdjustmentAdjuster>
            {
                new RoundingAssetReturnAdjustmentAdjuster(0.5),
                new UnitAssetReturnAdjustmentAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.92, ["central"] = 1.0, ["online"] = 0.88 }),
                new TieredAssetReturnAdjustmentAdjuster(
                    new List<double> { 3088.0, 5007.0 },
                    new List<double> { 0.82, 1.03 }),
            },
            new List<string>(),
            3.9,
            89969.3);
    }

    public double Compute(AssetReturnAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
