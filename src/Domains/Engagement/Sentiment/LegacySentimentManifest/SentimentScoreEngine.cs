namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment score calculation.</summary>
public interface ISentimentScoreStep
{
    string Describe();
    double Adjust(SentimentScore item, double valueValue);
}

public sealed record NameSentimentScoreAdjuster(Dictionary<string, double> Factors) : ISentimentScoreStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SentimentScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSentimentScoreAdjuster(Registry Flags, string Flag, double Percent) : ISentimentScoreStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSentimentScoreAdjuster(List<double> Bounds, List<double> Rates) : ISentimentScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentScore item, double valueValue)
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

/// <summary>Runs the sentiment score steps in order and keeps a trace of each.</summary>
public sealed class SentimentScoreEngine(List<ISentimentScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentScoreEngine DefaultSentimentScoreEngine(Registry flags)
    {
        return new SentimentScoreEngine(
            new List<ISentimentScoreStep>
            {
                new NameSentimentScoreAdjuster(new Dictionary<string, double> { ["north"] = 1.06, ["south"] = 0.77 }),
                new FlaggedSentimentScoreAdjuster(flags, "sentiment-score-pending", 6.1),
                new TieredSentimentScoreAdjuster(
                    new List<double> { 1858.0, 3927.0, 5010.0, 6616.0 },
                    new List<double> { 1.05, 1.06, 1.15, 1.06 }),
            },
            new List<string>(),
            3.8,
            85783.6);
    }

    public double Compute(SentimentScore item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
