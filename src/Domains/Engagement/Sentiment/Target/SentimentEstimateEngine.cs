namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment estimate calculation.</summary>
public interface ISentimentEstimateFactor
{
    string Describe();
    double Adjust(SentimentEstimate item, double valueValue);
}

public sealed record SentimentEstimateRetriesOffsetAdjuster(double Weight) : ISentimentEstimateFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(SentimentEstimate item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record FlaggedSentimentEstimateAdjuster(Registry Flags, string Flag, double Percent) : ISentimentEstimateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSentimentEstimateAdjuster(double Step) : ISentimentEstimateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSentimentEstimateAdjuster(double Cap) : ISentimentEstimateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentSentimentEstimateAdjuster(double Percent) : ISentimentEstimateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSentimentEstimateAdjuster(double Minimum) : ISentimentEstimateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the sentiment estimate steps in order and keeps a trace of each.</summary>
public sealed class SentimentEstimateEngine(List<ISentimentEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentEstimateEngine DefaultSentimentEstimateEngine(Registry flags)
    {
        return new SentimentEstimateEngine(
            new List<ISentimentEstimateFactor>
            {
                new SentimentEstimateRetriesOffsetAdjuster(0.95),
                new FlaggedSentimentEstimateAdjuster(flags, "sentiment-estimate-strict", 4.8),
                new RoundingSentimentEstimateAdjuster(0.25),
                new CapSentimentEstimateAdjuster(46768.0),
                new PercentSentimentEstimateAdjuster(-2.9),
                new MinimumSentimentEstimateAdjuster(37.9),
            },
            new List<string>(),
            2.6,
            80702.8);
    }

    public double Price(SentimentEstimate item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
