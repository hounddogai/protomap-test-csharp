namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched sentiment manifest calculation.</summary>
public interface IBatchedSentimentManifestFactor
{
    string Describe();
    double Adjust(BatchedSentimentManifest item, double valueValue);
}

public sealed record NameBatchedSentimentManifestAdjuster(Dictionary<string, double> Factors) : IBatchedSentimentManifestFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BatchedSentimentManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapBatchedSentimentManifestAdjuster(double Cap) : IBatchedSentimentManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedSentimentManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedSentimentManifestAdjuster(List<double> Bounds, List<double> Rates) : IBatchedSentimentManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedSentimentManifest item, double valueValue)
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

public sealed record PercentBatchedSentimentManifestAdjuster(double Percent) : IBatchedSentimentManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedSentimentManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBatchedSentimentManifestAdjuster(Registry Flags, string Flag, double Percent) : IBatchedSentimentManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedSentimentManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched sentiment manifest steps in order and keeps a trace of each.</summary>
public sealed class BatchedSentimentManifestEngine(List<IBatchedSentimentManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedSentimentManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedSentimentManifestEngine DefaultBatchedSentimentManifestEngine(Registry flags)
    {
        return new BatchedSentimentManifestEngine(
            new List<IBatchedSentimentManifestFactor>
            {
                new NameBatchedSentimentManifestAdjuster(new Dictionary<string, double> { ["south"] = 1.02 }),
                new CapBatchedSentimentManifestAdjuster(7721.0),
                new TieredBatchedSentimentManifestAdjuster(
                    new List<double> { 3087.0, 7349.0, 7837.0 },
                    new List<double> { 1.18, 0.89, 1.14 }),
                new PercentBatchedSentimentManifestAdjuster(18.6),
                new FlaggedBatchedSentimentManifestAdjuster(flags, "batched-sentiment-manifest-manual", -5.8),
            },
            new List<string>(),
            3.3,
            35509.9);
    }

    public double Run(BatchedSentimentManifest item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedSentimentManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
