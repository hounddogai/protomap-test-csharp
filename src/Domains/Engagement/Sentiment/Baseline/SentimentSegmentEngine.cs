namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment segment calculation.</summary>
public interface ISentimentSegmentStep
{
    string Describe();
    double Adjust(SentimentSegment item, double valueValue);
}

public sealed record MinimumSentimentSegmentAdjuster(double Minimum) : ISentimentSegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSentimentSegmentAdjuster(Registry Flags, string Flag, double Percent) : ISentimentSegmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSentimentSegmentAdjuster(double Percent) : ISentimentSegmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSentimentSegmentAdjuster(double Step) : ISentimentSegmentStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamSentimentSegmentAdjuster(Dictionary<string, double> Factors) : ISentimentSegmentStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SentimentSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment segment steps in order and keeps a trace of each.</summary>
public sealed class SentimentSegmentEngine(List<ISentimentSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentSegmentEngine DefaultSentimentSegmentEngine(Registry flags)
    {
        return new SentimentSegmentEngine(
            new List<ISentimentSegmentStep>
            {
                new MinimumSentimentSegmentAdjuster(27.5),
                new FlaggedSentimentSegmentAdjuster(flags, "sentiment-segment-fallback", 6.0),
                new PercentSentimentSegmentAdjuster(6.2),
                new RoundingSentimentSegmentAdjuster(0.1),
                new TeamSentimentSegmentAdjuster(new Dictionary<string, double> { ["west"] = 1.21 }),
            },
            new List<string>(),
            4.5,
            29483.4);
    }

    public double Run(SentimentSegment item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
