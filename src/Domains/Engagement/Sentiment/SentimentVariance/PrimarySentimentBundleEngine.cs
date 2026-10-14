namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary sentiment bundle calculation.</summary>
public interface IPrimarySentimentBundleFactor
{
    string Describe();
    double Adjust(PrimarySentimentBundle item, double valueValue);
}

public sealed record ChannelPrimarySentimentBundleAdjuster(Dictionary<string, double> Factors) : IPrimarySentimentBundleFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PrimarySentimentBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPrimarySentimentBundleAdjuster(Registry Flags, string Flag, double Percent) : IPrimarySentimentBundleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimarySentimentBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimarySentimentBundleAdjuster(double Percent) : IPrimarySentimentBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimarySentimentBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPrimarySentimentBundleAdjuster(double Step) : IPrimarySentimentBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimarySentimentBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPrimarySentimentBundleAdjuster(List<double> Bounds, List<double> Rates) : IPrimarySentimentBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimarySentimentBundle item, double valueValue)
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

public sealed record MinimumPrimarySentimentBundleAdjuster(double Minimum) : IPrimarySentimentBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimarySentimentBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the primary sentiment bundle steps in order and keeps a trace of each.</summary>
public sealed class PrimarySentimentBundleEngine(List<IPrimarySentimentBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimarySentimentBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimarySentimentBundleEngine DefaultPrimarySentimentBundleEngine(Registry flags)
    {
        return new PrimarySentimentBundleEngine(
            new List<IPrimarySentimentBundleFactor>
            {
                new ChannelPrimarySentimentBundleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.33, ["west"] = 0.96 }),
                new FlaggedPrimarySentimentBundleAdjuster(flags, "primary-sentiment-bundle-lenient", -1.7),
                new PercentPrimarySentimentBundleAdjuster(5.9),
                new RoundingPrimarySentimentBundleAdjuster(5.0),
                new TieredPrimarySentimentBundleAdjuster(
                    new List<double> { 2009.0, 7115.0 },
                    new List<double> { 0.94, 0.85 }),
                new MinimumPrimarySentimentBundleAdjuster(1.9),
            },
            new List<string>(),
            2.6,
            88684.8);
    }

    public double Price(PrimarySentimentBundle item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimarySentimentBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
