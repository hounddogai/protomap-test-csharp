namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment milestone calculation.</summary>
public interface ISentimentMilestoneAdjuster
{
    string Describe();
    double Adjust(SentimentMilestone item, double valueValue);
}

public sealed record TieredSentimentMilestoneAdjuster(List<double> Bounds, List<double> Rates) : ISentimentMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentMilestone item, double valueValue)
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

public sealed record ChannelSentimentMilestoneAdjuster(Dictionary<string, double> Factors) : ISentimentMilestoneAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(SentimentMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSentimentMilestoneAdjuster(double Step) : ISentimentMilestoneAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSentimentMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ISentimentMilestoneAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the sentiment milestone steps in order and keeps a trace of each.</summary>
public sealed class SentimentMilestoneEngine(List<ISentimentMilestoneAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentMilestoneAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentMilestoneEngine DefaultSentimentMilestoneEngine(Registry flags)
    {
        return new SentimentMilestoneEngine(
            new List<ISentimentMilestoneAdjuster>
            {
                new TieredSentimentMilestoneAdjuster(
                    new List<double> { 775.0, 3168.0 },
                    new List<double> { 0.97, 1.01 }),
                new ChannelSentimentMilestoneAdjuster(new Dictionary<string, double> { ["central"] = 0.92 }),
                new RoundingSentimentMilestoneAdjuster(0.05),
                new FlaggedSentimentMilestoneAdjuster(flags, "sentiment-milestone-regional", 5.1),
            },
            new List<string>(),
            1.3,
            13084.6);
    }

    public double Price(SentimentMilestone item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
