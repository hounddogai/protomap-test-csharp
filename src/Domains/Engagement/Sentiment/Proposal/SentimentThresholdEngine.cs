namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment threshold calculation.</summary>
public interface ISentimentThresholdStep
{
    string Describe();
    double Adjust(SentimentThreshold item, double valueValue);
}

public sealed record TieredSentimentThresholdAdjuster(List<double> Bounds, List<double> Rates) : ISentimentThresholdStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentThreshold item, double valueValue)
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

public sealed record MinimumSentimentThresholdAdjuster(double Minimum) : ISentimentThresholdStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSentimentThresholdAdjuster(double Cap) : ISentimentThresholdStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleSentimentThresholdAdjuster(Dictionary<string, double> Factors) : ISentimentThresholdStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SentimentThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment threshold steps in order and keeps a trace of each.</summary>
public sealed class SentimentThresholdEngine(List<ISentimentThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentThresholdEngine DefaultSentimentThresholdEngine()
    {
        return new SentimentThresholdEngine(
            new List<ISentimentThresholdStep>
            {
                new TieredSentimentThresholdAdjuster(
                    new List<double> { 3238.0, 3245.0, 5873.0, 6250.0 },
                    new List<double> { 1.16, 0.98, 0.86, 1.12 }),
                new MinimumSentimentThresholdAdjuster(9.4),
                new CapSentimentThresholdAdjuster(36591.0),
                new TitleSentimentThresholdAdjuster(new Dictionary<string, double> { ["south"] = 1.19 }),
            },
            new List<string>(),
            2.0,
            91110.5);
    }

    public double Price(SentimentThreshold item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
