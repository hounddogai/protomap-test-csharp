namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment forecast calculation.</summary>
public interface ISentimentForecastFactor
{
    string Describe();
    double Adjust(SentimentForecast item, double valueValue);
}

public sealed record RoundingSentimentForecastAdjuster(double Step) : ISentimentForecastFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentForecast item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleSentimentForecastAdjuster(Dictionary<string, double> Factors) : ISentimentForecastFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SentimentForecast item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSentimentForecastAdjuster(List<double> Bounds, List<double> Rates) : ISentimentForecastFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentForecast item, double valueValue)
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

/// <summary>Runs the sentiment forecast steps in order and keeps a trace of each.</summary>
public sealed class SentimentForecastEngine(List<ISentimentForecastFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentForecastFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentForecastEngine DefaultSentimentForecastEngine()
    {
        return new SentimentForecastEngine(
            new List<ISentimentForecastFactor>
            {
                new RoundingSentimentForecastAdjuster(0.5),
                new TitleSentimentForecastAdjuster(new Dictionary<string, double> { ["east"] = 1.02 }),
                new TieredSentimentForecastAdjuster(
                    new List<double> { 2166.0, 3422.0 },
                    new List<double> { 1.11, 0.96 }),
            },
            new List<string>(),
            0.5,
            87054.8);
    }

    public double Run(SentimentForecast item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentForecast> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
