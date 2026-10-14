namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal sentiment draft calculation.</summary>
public interface ISeasonalSentimentDraftFactor
{
    string Describe();
    double Adjust(SeasonalSentimentDraft item, double valueValue);
}

public sealed record TieredSeasonalSentimentDraftAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalSentimentDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalSentimentDraft item, double valueValue)
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

public sealed record FlaggedSeasonalSentimentDraftAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalSentimentDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalSentimentDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSeasonalSentimentDraftAdjuster(double Minimum) : ISeasonalSentimentDraftFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalSentimentDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SeasonalSentimentDraftQuantityOffsetAdjuster(double Weight) : ISeasonalSentimentDraftFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(SeasonalSentimentDraft item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record LabelSeasonalSentimentDraftAdjuster(Dictionary<string, double> Factors) : ISeasonalSentimentDraftFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalSentimentDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSeasonalSentimentDraftAdjuster(double Cap) : ISeasonalSentimentDraftFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalSentimentDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal sentiment draft steps in order and keeps a trace of each.</summary>
public sealed class SeasonalSentimentDraftEngine(List<ISeasonalSentimentDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalSentimentDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalSentimentDraftEngine DefaultSeasonalSentimentDraftEngine(Registry flags)
    {
        return new SeasonalSentimentDraftEngine(
            new List<ISeasonalSentimentDraftFactor>
            {
                new TieredSeasonalSentimentDraftAdjuster(
                    new List<double> { 3984.0, 5180.0, 6718.0, 9171.0 },
                    new List<double> { 0.97, 1.07, 0.84, 1.02 }),
                new FlaggedSeasonalSentimentDraftAdjuster(flags, "seasonal-sentiment-draft-adaptive", 0.1),
                new MinimumSeasonalSentimentDraftAdjuster(48.9),
                new SeasonalSentimentDraftQuantityOffsetAdjuster(0.28),
                new LabelSeasonalSentimentDraftAdjuster(new Dictionary<string, double> { ["retail"] = 1.01 }),
                new CapSeasonalSentimentDraftAdjuster(43629.0),
            },
            new List<string>(),
            4.3,
            40282.6);
    }

    public double Run(SeasonalSentimentDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalSentimentDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
