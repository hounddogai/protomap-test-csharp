namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment preference calculation.</summary>
public interface ISentimentPreferenceModifier
{
    string Describe();
    double Adjust(SentimentPreference item, double valueValue);
}

public sealed record SentimentPreferenceQuantityOffsetAdjuster(double Weight) : ISentimentPreferenceModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(SentimentPreference item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record PercentSentimentPreferenceAdjuster(double Percent) : ISentimentPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSentimentPreferenceAdjuster(double Cap) : ISentimentPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSentimentPreferenceAdjuster(double Step) : ISentimentPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSentimentPreferenceAdjuster(List<double> Bounds, List<double> Rates) : ISentimentPreferenceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentPreference item, double valueValue)
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

/// <summary>Runs the sentiment preference steps in order and keeps a trace of each.</summary>
public sealed class SentimentPreferenceEngine(List<ISentimentPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentPreferenceEngine DefaultSentimentPreferenceEngine()
    {
        return new SentimentPreferenceEngine(
            new List<ISentimentPreferenceModifier>
            {
                new SentimentPreferenceQuantityOffsetAdjuster(1.71),
                new PercentSentimentPreferenceAdjuster(-3.3),
                new CapSentimentPreferenceAdjuster(11675.0),
                new RoundingSentimentPreferenceAdjuster(1.0),
                new TieredSentimentPreferenceAdjuster(
                    new List<double> { 9528.0, 9637.0 },
                    new List<double> { 1.18, 0.94 }),
            },
            new List<string>(),
            4.3,
            40961.4);
    }

    public double Price(SentimentPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
