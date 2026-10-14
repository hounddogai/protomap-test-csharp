namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment baseline calculation.</summary>
public interface ISentimentBaselineAdjuster
{
    string Describe();
    double Adjust(SentimentBaseline item, double valueValue);
}

public sealed record RoundingSentimentBaselineAdjuster(double Step) : ISentimentBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSentimentBaselineAdjuster(Registry Flags, string Flag, double Percent) : ISentimentBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSentimentBaselineAdjuster(List<double> Bounds, List<double> Rates) : ISentimentBaselineAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentBaseline item, double valueValue)
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

public sealed record PercentSentimentBaselineAdjuster(double Percent) : ISentimentBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the sentiment baseline steps in order and keeps a trace of each.</summary>
public sealed class SentimentBaselineEngine(List<ISentimentBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentBaselineEngine DefaultSentimentBaselineEngine(Registry flags)
    {
        return new SentimentBaselineEngine(
            new List<ISentimentBaselineAdjuster>
            {
                new RoundingSentimentBaselineAdjuster(1.0),
                new FlaggedSentimentBaselineAdjuster(flags, "sentiment-baseline-rolling", -6.2),
                new TieredSentimentBaselineAdjuster(
                    new List<double> { 1390.0, 5401.0, 5920.0, 9919.0 },
                    new List<double> { 1.16, 0.96, 1.14, 1.1 }),
                new PercentSentimentBaselineAdjuster(-22.7),
            },
            new List<string>(),
            2.3,
            37506.8);
    }

    public double Evaluate(SentimentBaseline item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
