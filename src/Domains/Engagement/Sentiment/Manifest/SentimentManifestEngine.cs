namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment manifest calculation.</summary>
public interface ISentimentManifestModifier
{
    string Describe();
    double Adjust(SentimentManifest item, double valueValue);
}

public sealed record PercentSentimentManifestAdjuster(double Percent) : ISentimentManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSentimentManifestAdjuster(double Cap) : ISentimentManifestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSentimentManifestAdjuster(double Step) : ISentimentManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategorySentimentManifestAdjuster(Dictionary<string, double> Factors) : ISentimentManifestModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SentimentManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSentimentManifestAdjuster(double Minimum) : ISentimentManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSentimentManifestAdjuster(List<double> Bounds, List<double> Rates) : ISentimentManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentManifest item, double valueValue)
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

/// <summary>Runs the sentiment manifest steps in order and keeps a trace of each.</summary>
public sealed class SentimentManifestEngine(List<ISentimentManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentManifestEngine DefaultSentimentManifestEngine()
    {
        return new SentimentManifestEngine(
            new List<ISentimentManifestModifier>
            {
                new PercentSentimentManifestAdjuster(-7.9),
                new CapSentimentManifestAdjuster(49143.0),
                new RoundingSentimentManifestAdjuster(0.5),
                new CategorySentimentManifestAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.22, ["online"] = 1.33 }),
                new MinimumSentimentManifestAdjuster(19.6),
                new TieredSentimentManifestAdjuster(
                    new List<double> { 384.0, 4776.0 },
                    new List<double> { 0.91, 0.83 }),
            },
            new List<string>(),
            0.6,
            72092.0);
    }

    public double Evaluate(SentimentManifest item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
