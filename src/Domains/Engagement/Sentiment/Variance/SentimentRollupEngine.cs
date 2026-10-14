namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment rollup calculation.</summary>
public interface ISentimentRollupStep
{
    string Describe();
    double Adjust(SentimentRollup item, double valueValue);
}

public sealed record MinimumSentimentRollupAdjuster(double Minimum) : ISentimentRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSentimentRollupAdjuster(List<double> Bounds, List<double> Rates) : ISentimentRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentRollup item, double valueValue)
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

public sealed record CapSentimentRollupAdjuster(double Cap) : ISentimentRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSentimentRollupAdjuster(Registry Flags, string Flag, double Percent) : ISentimentRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSentimentRollupAdjuster(double Step) : ISentimentRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentSentimentRollupAdjuster(double Percent) : ISentimentRollupStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the sentiment rollup steps in order and keeps a trace of each.</summary>
public sealed class SentimentRollupEngine(List<ISentimentRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentRollupEngine DefaultSentimentRollupEngine(Registry flags)
    {
        return new SentimentRollupEngine(
            new List<ISentimentRollupStep>
            {
                new MinimumSentimentRollupAdjuster(37.3),
                new TieredSentimentRollupAdjuster(
                    new List<double> { 8027.0, 8310.0 },
                    new List<double> { 1.0, 0.94 }),
                new CapSentimentRollupAdjuster(6893.0),
                new FlaggedSentimentRollupAdjuster(flags, "sentiment-rollup-regional", 8.1),
                new RoundingSentimentRollupAdjuster(0.25),
                new PercentSentimentRollupAdjuster(19.2),
            },
            new List<string>(),
            0.9,
            1009.6);
    }

    public double Run(SentimentRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
