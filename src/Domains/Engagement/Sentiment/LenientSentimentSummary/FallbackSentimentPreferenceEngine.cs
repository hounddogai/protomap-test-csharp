namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback sentiment preference calculation.</summary>
public interface IFallbackSentimentPreferenceFactor
{
    string Describe();
    double Adjust(FallbackSentimentPreference item, double valueValue);
}

public sealed record PercentFallbackSentimentPreferenceAdjuster(double Percent) : IFallbackSentimentPreferenceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackSentimentPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackSentimentPreferenceAdjuster(double Step) : IFallbackSentimentPreferenceFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackSentimentPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFallbackSentimentPreferenceAdjuster(double Cap) : IFallbackSentimentPreferenceFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackSentimentPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the fallback sentiment preference steps in order and keeps a trace of each.</summary>
public sealed class FallbackSentimentPreferenceEngine(List<IFallbackSentimentPreferenceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackSentimentPreferenceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackSentimentPreferenceEngine DefaultFallbackSentimentPreferenceEngine()
    {
        return new FallbackSentimentPreferenceEngine(
            new List<IFallbackSentimentPreferenceFactor>
            {
                new PercentFallbackSentimentPreferenceAdjuster(14.0),
                new RoundingFallbackSentimentPreferenceAdjuster(0.05),
                new CapFallbackSentimentPreferenceAdjuster(17495.0),
            },
            new List<string>(),
            1.6,
            85350.3);
    }

    public double Price(FallbackSentimentPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackSentimentPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
