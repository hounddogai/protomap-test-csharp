namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback sentiment bundle calculation.</summary>
public interface IFallbackSentimentBundleFactor
{
    string Describe();
    double Adjust(FallbackSentimentBundle item, double valueValue);
}

public sealed record RoundingFallbackSentimentBundleAdjuster(double Step) : IFallbackSentimentBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackSentimentBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleFallbackSentimentBundleAdjuster(Dictionary<string, double> Factors) : IFallbackSentimentBundleFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FallbackSentimentBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentFallbackSentimentBundleAdjuster(double Percent) : IFallbackSentimentBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackSentimentBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFallbackSentimentBundleAdjuster(Registry Flags, string Flag, double Percent) : IFallbackSentimentBundleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackSentimentBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback sentiment bundle steps in order and keeps a trace of each.</summary>
public sealed class FallbackSentimentBundleEngine(List<IFallbackSentimentBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackSentimentBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackSentimentBundleEngine DefaultFallbackSentimentBundleEngine(Registry flags)
    {
        return new FallbackSentimentBundleEngine(
            new List<IFallbackSentimentBundleFactor>
            {
                new RoundingFallbackSentimentBundleAdjuster(0.05),
                new TitleFallbackSentimentBundleAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.19, ["west"] = 1.29 }),
                new PercentFallbackSentimentBundleAdjuster(10.0),
                new FlaggedFallbackSentimentBundleAdjuster(flags, "fallback-sentiment-bundle-regional", -8.7),
            },
            new List<string>(),
            0.5,
            92339.3);
    }

    public double Run(FallbackSentimentBundle item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackSentimentBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
