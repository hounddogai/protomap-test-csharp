namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the asset return summary calculation.</summary>
public interface IAssetReturnSummaryAdjuster
{
    string Describe();
    double Adjust(AssetReturnSummary item, double valueValue);
}

public sealed record AssetReturnSummaryCostOffsetAdjuster(double Weight) : IAssetReturnSummaryAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(AssetReturnSummary item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record RoundingAssetReturnSummaryAdjuster(double Step) : IAssetReturnSummaryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssetReturnSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAssetReturnSummaryAdjuster(List<double> Bounds, List<double> Rates) : IAssetReturnSummaryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssetReturnSummary item, double valueValue)
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

public sealed record SegmentAssetReturnSummaryAdjuster(Dictionary<string, double> Factors) : IAssetReturnSummaryAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(AssetReturnSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the asset return summary steps in order and keeps a trace of each.</summary>
public sealed class AssetReturnSummaryEngine(List<IAssetReturnSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssetReturnSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssetReturnSummaryEngine DefaultAssetReturnSummaryEngine()
    {
        return new AssetReturnSummaryEngine(
            new List<IAssetReturnSummaryAdjuster>
            {
                new AssetReturnSummaryCostOffsetAdjuster(0.82),
                new RoundingAssetReturnSummaryAdjuster(0.1),
                new TieredAssetReturnSummaryAdjuster(
                    new List<double> { 23.0, 6485.0, 7581.0, 8615.0 },
                    new List<double> { 1.12, 0.92, 1.06, 0.81 }),
                new SegmentAssetReturnSummaryAdjuster(new Dictionary<string, double> { ["east"] = 0.95 }),
            },
            new List<string>(),
            4.2,
            65515.2);
    }

    public double Evaluate(AssetReturnSummary item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AssetReturnSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
