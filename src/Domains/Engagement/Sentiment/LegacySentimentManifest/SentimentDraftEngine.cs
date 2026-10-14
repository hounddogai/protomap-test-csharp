namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment draft calculation.</summary>
public interface ISentimentDraftModifier
{
    string Describe();
    double Adjust(SentimentDraft item, double valueValue);
}

public sealed record MinimumSentimentDraftAdjuster(double Minimum) : ISentimentDraftModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSentimentDraftAdjuster(double Percent) : ISentimentDraftModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SentimentDraftCapacityOffsetAdjuster(double Weight) : ISentimentDraftModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(SentimentDraft item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record TieredSentimentDraftAdjuster(List<double> Bounds, List<double> Rates) : ISentimentDraftModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentDraft item, double valueValue)
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

public sealed record FlaggedSentimentDraftAdjuster(Registry Flags, string Flag, double Percent) : ISentimentDraftModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSentimentDraftAdjuster(double Cap) : ISentimentDraftModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the sentiment draft steps in order and keeps a trace of each.</summary>
public sealed class SentimentDraftEngine(List<ISentimentDraftModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentDraftModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentDraftEngine DefaultSentimentDraftEngine(Registry flags)
    {
        return new SentimentDraftEngine(
            new List<ISentimentDraftModifier>
            {
                new MinimumSentimentDraftAdjuster(47.8),
                new PercentSentimentDraftAdjuster(1.1),
                new SentimentDraftCapacityOffsetAdjuster(1.86),
                new TieredSentimentDraftAdjuster(
                    new List<double> { 2090.0, 4717.0 },
                    new List<double> { 1.14, 1.04 }),
                new FlaggedSentimentDraftAdjuster(flags, "sentiment-draft-fallback", -3.2),
                new CapSentimentDraftAdjuster(3946.0),
            },
            new List<string>(),
            1.9,
            20021.9);
    }

    public double Evaluate(SentimentDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
