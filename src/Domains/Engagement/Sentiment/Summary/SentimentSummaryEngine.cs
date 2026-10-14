namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment summary calculation.</summary>
public interface ISentimentSummaryModifier
{
    string Describe();
    double Adjust(SentimentSummary item, double valueValue);
}

public sealed record PercentSentimentSummaryAdjuster(double Percent) : ISentimentSummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleSentimentSummaryAdjuster(Dictionary<string, double> Factors) : ISentimentSummaryModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SentimentSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSentimentSummaryAdjuster(double Minimum) : ISentimentSummaryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSentimentSummaryAdjuster(Registry Flags, string Flag, double Percent) : ISentimentSummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the sentiment summary steps in order and keeps a trace of each.</summary>
public sealed class SentimentSummaryEngine(List<ISentimentSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentSummaryEngine DefaultSentimentSummaryEngine(Registry flags)
    {
        return new SentimentSummaryEngine(
            new List<ISentimentSummaryModifier>
            {
                new PercentSentimentSummaryAdjuster(15.4),
                new TitleSentimentSummaryAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.3, ["online"] = 0.79, ["north"] = 1.0 }),
                new MinimumSentimentSummaryAdjuster(8.3),
                new FlaggedSentimentSummaryAdjuster(flags, "sentiment-summary-weighted", 7.4),
            },
            new List<string>(),
            1.8,
            46294.9);
    }

    public double Compute(SentimentSummary item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
