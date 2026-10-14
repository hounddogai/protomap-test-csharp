namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered sentiment manifest calculation.</summary>
public interface ITieredSentimentManifestModifier
{
    string Describe();
    double Adjust(TieredSentimentManifest item, double valueValue);
}

public sealed record MinimumTieredSentimentManifestAdjuster(double Minimum) : ITieredSentimentManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredSentimentManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTieredSentimentManifestAdjuster(List<double> Bounds, List<double> Rates) : ITieredSentimentManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredSentimentManifest item, double valueValue)
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

public sealed record RoundingTieredSentimentManifestAdjuster(double Step) : ITieredSentimentManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredSentimentManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered sentiment manifest steps in order and keeps a trace of each.</summary>
public sealed class TieredSentimentManifestEngine(List<ITieredSentimentManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredSentimentManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredSentimentManifestEngine DefaultTieredSentimentManifestEngine()
    {
        return new TieredSentimentManifestEngine(
            new List<ITieredSentimentManifestModifier>
            {
                new MinimumTieredSentimentManifestAdjuster(45.6),
                new TieredTieredSentimentManifestAdjuster(
                    new List<double> { 493.0, 733.0, 3208.0, 8470.0 },
                    new List<double> { 1.15, 0.93, 1.02, 1.14 }),
                new RoundingTieredSentimentManifestAdjuster(0.05),
            },
            new List<string>(),
            1.6,
            34040.2);
    }

    public double Price(TieredSentimentManifest item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<TieredSentimentManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
