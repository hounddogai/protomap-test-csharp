namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment claim calculation.</summary>
public interface ISentimentClaimModifier
{
    string Describe();
    double Adjust(SentimentClaim item, double valueValue);
}

public sealed record MinimumSentimentClaimAdjuster(double Minimum) : ISentimentClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSentimentClaimAdjuster(Registry Flags, string Flag, double Percent) : ISentimentClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSentimentClaimAdjuster(double Percent) : ISentimentClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SentimentClaimScoreOffsetAdjuster(double Weight) : ISentimentClaimModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(SentimentClaim item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record CategorySentimentClaimAdjuster(Dictionary<string, double> Factors) : ISentimentClaimModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SentimentClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment claim steps in order and keeps a trace of each.</summary>
public sealed class SentimentClaimEngine(List<ISentimentClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentClaimEngine DefaultSentimentClaimEngine(Registry flags)
    {
        return new SentimentClaimEngine(
            new List<ISentimentClaimModifier>
            {
                new MinimumSentimentClaimAdjuster(5.1),
                new FlaggedSentimentClaimAdjuster(flags, "sentiment-claim-tiered", 4.3),
                new PercentSentimentClaimAdjuster(-8.5),
                new SentimentClaimScoreOffsetAdjuster(0.94),
                new CategorySentimentClaimAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.24, ["north"] = 1.1 }),
            },
            new List<string>(),
            5.0,
            26255.3);
    }

    public double Run(SentimentClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
