namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched sentiment bundle calculation.</summary>
public interface IBatchedSentimentBundleFactor
{
    string Describe();
    double Adjust(BatchedSentimentBundle item, double valueValue);
}

public sealed record LabelBatchedSentimentBundleAdjuster(Dictionary<string, double> Factors) : IBatchedSentimentBundleFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BatchedSentimentBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedSentimentBundleAdjuster(Registry Flags, string Flag, double Percent) : IBatchedSentimentBundleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedSentimentBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedSentimentBundleAdjuster(double Cap) : IBatchedSentimentBundleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedSentimentBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched sentiment bundle steps in order and keeps a trace of each.</summary>
public sealed class BatchedSentimentBundleEngine(List<IBatchedSentimentBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedSentimentBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedSentimentBundleEngine DefaultBatchedSentimentBundleEngine(Registry flags)
    {
        return new BatchedSentimentBundleEngine(
            new List<IBatchedSentimentBundleFactor>
            {
                new LabelBatchedSentimentBundleAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.93, ["east"] = 0.95 }),
                new FlaggedBatchedSentimentBundleAdjuster(flags, "batched-sentiment-bundle-rolling", -3.2),
                new CapBatchedSentimentBundleAdjuster(9810.0),
            },
            new List<string>(),
            2.4,
            2610.2);
    }

    public double Compute(BatchedSentimentBundle item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedSentimentBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
