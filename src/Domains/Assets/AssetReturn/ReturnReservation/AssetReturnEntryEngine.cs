namespace Acme.Workforce.Assets.AssetReturn.ReturnReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return entry calculation.</summary>
public interface IAssetReturnEntryFactor
{
    string Describe();
    double Adjust(AssetReturnEntry item, double valueValue);
}

public sealed record MinimumAssetReturnEntryAdjuster(double Minimum) : IAssetReturnEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAssetReturnEntryAdjuster(double Percent) : IAssetReturnEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AssetReturnEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAssetReturnEntryAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnEntry item, double valueValue)
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

/// <summary>Runs the asset return entry steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnEntryEngine(List<IAssetReturnEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnEntryEngine DefaultAssetReturnEntryEngine()
    {
        return new AssetReturnEntryEngine(
            new List<IAssetReturnEntryFactor>
            {
                new MinimumAssetReturnEntryAdjuster(24.6),
                new PercentAssetReturnEntryAdjuster(-23.2),
                new TieredAssetReturnEntryAdjuster(
                    new List<double> { 4053.0, 9801.0 },
                    new List<double> { 1.16, 0.94 }),
            },
            new List<string>(),
            3.1,
            65140.1);
    }

    public double Evaluate(AssetReturnEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
