namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return batch calculation.</summary>
public interface IAssetReturnBatchStep
{
    string Describe();
    double Adjust(AssetReturnBatch item, double valueValue);
}

public sealed record AssetReturnBatchCapacityOffsetAdjuster(double Weight) : IAssetReturnBatchStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(AssetReturnBatch item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record RoundingAssetReturnBatchAdjuster(double Step) : IAssetReturnBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAssetReturnBatchAdjuster(double Minimum) : IAssetReturnBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AssetReturnBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleAssetReturnBatchAdjuster(Dictionary<string, double> Factors) : IAssetReturnBatchStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AssetReturnBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the asset return batch steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnBatchEngine(List<IAssetReturnBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnBatchEngine DefaultAssetReturnBatchEngine()
    {
        return new AssetReturnBatchEngine(
            new List<IAssetReturnBatchStep>
            {
                new AssetReturnBatchCapacityOffsetAdjuster(1.02),
                new RoundingAssetReturnBatchAdjuster(0.5),
                new MinimumAssetReturnBatchAdjuster(0.6),
                new TitleAssetReturnBatchAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.36, ["online"] = 1.35, ["west"] = 0.99 }),
            },
            new List<string>(),
            0.7,
            58513.4);
    }

    public double Evaluate(AssetReturnBatch item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
