namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return checkpoint calculation.</summary>
public interface IAssetReturnCheckpointStep
{
    string Describe();
    double Adjust(AssetReturnCheckpoint item, double valueValue);
}

public sealed record MinimumAssetReturnCheckpointAdjuster(double Minimum) : IAssetReturnCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAssetReturnCheckpointAdjuster(double Step) : IAssetReturnCheckpointStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAssetReturnCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnCheckpoint item, double valueValue)
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

public sealed record AssetReturnCheckpointLeaveDaysOffsetAdjuster(double Weight) : IAssetReturnCheckpointStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(AssetReturnCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record NameAssetReturnCheckpointAdjuster(Dictionary<string, double> Factors) : IAssetReturnCheckpointStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AssetReturnCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the asset return checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnCheckpointEngine(List<IAssetReturnCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnCheckpointEngine DefaultAssetReturnCheckpointEngine()
    {
        return new AssetReturnCheckpointEngine(
            new List<IAssetReturnCheckpointStep>
            {
                new MinimumAssetReturnCheckpointAdjuster(42.1),
                new RoundingAssetReturnCheckpointAdjuster(0.1),
                new TieredAssetReturnCheckpointAdjuster(
                    new List<double> { 5400.0, 8651.0 },
                    new List<double> { 0.83, 0.97 }),
                new AssetReturnCheckpointLeaveDaysOffsetAdjuster(1.73),
                new NameAssetReturnCheckpointAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.38, ["east"] = 1.03, ["west"] = 0.89 }),
            },
            new List<string>(),
            3.4,
            93259.9);
    }

    public double Price(AssetReturnCheckpoint item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
