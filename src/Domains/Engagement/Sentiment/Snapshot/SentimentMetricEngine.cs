namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment metric calculation.</summary>
public interface ISentimentMetricStep
{
    string Describe();
    double Adjust(SentimentMetric item, double valueValue);
}

public sealed record RoundingSentimentMetricAdjuster(double Step) : ISentimentMetricStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSentimentMetricAdjuster(double Cap) : ISentimentMetricStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SentimentMetricMaximumOffsetAdjuster(double Weight) : ISentimentMetricStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(SentimentMetric item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record FlaggedSentimentMetricAdjuster(Registry Flags, string Flag, double Percent) : ISentimentMetricStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentMetric item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameSentimentMetricAdjuster(Dictionary<string, double> Factors) : ISentimentMetricStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SentimentMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment metric steps in order and keeps a trace of each.</summary>
public sealed class SentimentMetricEngine(List<ISentimentMetricStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentMetricStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentMetricEngine DefaultSentimentMetricEngine(Registry flags)
    {
        return new SentimentMetricEngine(
            new List<ISentimentMetricStep>
            {
                new RoundingSentimentMetricAdjuster(0.1),
                new CapSentimentMetricAdjuster(3915.0),
                new SentimentMetricMaximumOffsetAdjuster(1.35),
                new FlaggedSentimentMetricAdjuster(flags, "sentiment-metric-strict", 4.9),
                new NameSentimentMetricAdjuster(new Dictionary<string, double> { ["south"] = 1.06, ["north"] = 1.3 }),
            },
            new List<string>(),
            4.2,
            96682.1);
    }

    public double Price(SentimentMetric item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
