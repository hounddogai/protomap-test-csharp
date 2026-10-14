namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment batch calculation.</summary>
public interface ISentimentBatchModifier
{
    string Describe();
    double Adjust(SentimentBatch item, double valueValue);
}

public sealed record MinimumSentimentBatchAdjuster(double Minimum) : ISentimentBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSentimentBatchAdjuster(Registry Flags, string Flag, double Percent) : ISentimentBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSentimentBatchAdjuster(double Step) : ISentimentBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SentimentBatchLimitOffsetAdjuster(double Weight) : ISentimentBatchModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(SentimentBatch item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentSentimentBatchAdjuster(double Percent) : ISentimentBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentSentimentBatchAdjuster(Dictionary<string, double> Factors) : ISentimentBatchModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(SentimentBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment batch steps in order and keeps a trace of each.</summary>
public sealed class SentimentBatchEngine(List<ISentimentBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentBatchEngine DefaultSentimentBatchEngine(Registry flags)
    {
        return new SentimentBatchEngine(
            new List<ISentimentBatchModifier>
            {
                new MinimumSentimentBatchAdjuster(14.2),
                new FlaggedSentimentBatchAdjuster(flags, "sentiment-batch-seasonal", -4.3),
                new RoundingSentimentBatchAdjuster(5.0),
                new SentimentBatchLimitOffsetAdjuster(1.71),
                new PercentSentimentBatchAdjuster(-12.6),
                new SegmentSentimentBatchAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.85, ["online"] = 0.86, ["north"] = 1.25 }),
            },
            new List<string>(),
            0.6,
            8042.7);
    }

    public double Evaluate(SentimentBatch item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
