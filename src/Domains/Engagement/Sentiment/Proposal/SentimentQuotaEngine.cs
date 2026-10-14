namespace Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>One step of the sentiment quota calculation.</summary>
public interface ISentimentQuotaAdjuster
{
    string Describe();
    double Adjust(SentimentQuota item, double valueValue);
}

public sealed record MinimumSentimentQuotaAdjuster(double Minimum) : ISentimentQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSentimentQuotaAdjuster(double Percent) : ISentimentQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitSentimentQuotaAdjuster(Dictionary<string, double> Factors) : ISentimentQuotaAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(SentimentQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSentimentQuotaAdjuster(double Cap) : ISentimentQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the sentiment quota steps in order and keeps a trace of each.</summary>
public sealed class SentimentQuotaEngine(List<ISentimentQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentQuotaEngine DefaultSentimentQuotaEngine()
    {
        return new SentimentQuotaEngine(
            new List<ISentimentQuotaAdjuster>
            {
                new MinimumSentimentQuotaAdjuster(21.7),
                new PercentSentimentQuotaAdjuster(1.0),
                new UnitSentimentQuotaAdjuster(new Dictionary<string, double> { ["south"] = 0.79 }),
                new CapSentimentQuotaAdjuster(19605.0),
            },
            new List<string>(),
            0.6,
            24843.7);
    }

    public double Price(SentimentQuota item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
