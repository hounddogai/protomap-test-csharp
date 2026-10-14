namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment entry calculation.</summary>
public interface ISentimentEntryAdjuster
{
    string Describe();
    double Adjust(SentimentEntry item, double valueValue);
}

public sealed record TieredSentimentEntryAdjuster(List<double> Bounds, List<double> Rates) : ISentimentEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentEntry item, double valueValue)
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

public sealed record SentimentEntryGraceDaysOffsetAdjuster(double Weight) : ISentimentEntryAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(SentimentEntry item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record RoundingSentimentEntryAdjuster(double Step) : ISentimentEntryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the sentiment entry steps in order and keeps a trace of each.</summary>
public sealed class SentimentEntryEngine(List<ISentimentEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentEntryEngine DefaultSentimentEntryEngine()
    {
        return new SentimentEntryEngine(
            new List<ISentimentEntryAdjuster>
            {
                new TieredSentimentEntryAdjuster(
                    new List<double> { 5169.0, 6454.0 },
                    new List<double> { 1.16, 0.91 }),
                new SentimentEntryGraceDaysOffsetAdjuster(1.07),
                new RoundingSentimentEntryAdjuster(0.05),
            },
            new List<string>(),
            3.3,
            82716.4);
    }

    public double Price(SentimentEntry item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
