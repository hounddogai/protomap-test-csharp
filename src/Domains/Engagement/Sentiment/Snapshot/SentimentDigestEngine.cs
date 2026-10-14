namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>One step of the sentiment digest calculation.</summary>
public interface ISentimentDigestFactor
{
    string Describe();
    double Adjust(SentimentDigest item, double valueValue);
}

public sealed record TitleSentimentDigestAdjuster(Dictionary<string, double> Factors) : ISentimentDigestFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SentimentDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSentimentDigestAdjuster(double Cap) : ISentimentDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SentimentDigestSequenceOffsetAdjuster(double Weight) : ISentimentDigestFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(SentimentDigest item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record PercentSentimentDigestAdjuster(double Percent) : ISentimentDigestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the sentiment digest steps in order and keeps a trace of each.</summary>
public sealed class SentimentDigestEngine(List<ISentimentDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentDigestEngine DefaultSentimentDigestEngine()
    {
        return new SentimentDigestEngine(
            new List<ISentimentDigestFactor>
            {
                new TitleSentimentDigestAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.32, ["online"] = 1.1, ["retail"] = 1.3 }),
                new CapSentimentDigestAdjuster(3609.0),
                new SentimentDigestSequenceOffsetAdjuster(1.2),
                new PercentSentimentDigestAdjuster(5.3),
            },
            new List<string>(),
            2.0,
            4720.1);
    }

    public double Run(SentimentDigest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
