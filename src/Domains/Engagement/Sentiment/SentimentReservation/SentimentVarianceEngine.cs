namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment variance calculation.</summary>
public interface ISentimentVarianceAdjuster
{
    string Describe();
    double Adjust(SentimentVariance item, double valueValue);
}

public sealed record RoundingSentimentVarianceAdjuster(double Step) : ISentimentVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSentimentVarianceAdjuster(List<double> Bounds, List<double> Rates) : ISentimentVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentVariance item, double valueValue)
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

public sealed record CapSentimentVarianceAdjuster(double Cap) : ISentimentVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSentimentVarianceAdjuster(Registry Flags, string Flag, double Percent) : ISentimentVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSentimentVarianceAdjuster(double Minimum) : ISentimentVarianceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the sentiment variance steps in order and keeps a trace of each.</summary>
public sealed class SentimentVarianceEngine(List<ISentimentVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentVarianceEngine DefaultSentimentVarianceEngine(Registry flags)
    {
        return new SentimentVarianceEngine(
            new List<ISentimentVarianceAdjuster>
            {
                new RoundingSentimentVarianceAdjuster(0.5),
                new TieredSentimentVarianceAdjuster(
                    new List<double> { 900.0, 4261.0, 5402.0, 9890.0 },
                    new List<double> { 1.09, 1.14, 1.18, 0.94 }),
                new CapSentimentVarianceAdjuster(18333.0),
                new FlaggedSentimentVarianceAdjuster(flags, "sentiment-variance-strict", -5.3),
                new MinimumSentimentVarianceAdjuster(44.8),
            },
            new List<string>(),
            2.3,
            48624.8);
    }

    public double Compute(SentimentVariance item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
