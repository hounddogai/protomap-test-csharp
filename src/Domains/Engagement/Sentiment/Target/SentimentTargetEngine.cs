namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment target calculation.</summary>
public interface ISentimentTargetModifier
{
    string Describe();
    double Adjust(SentimentTarget item, double valueValue);
}

public sealed record TieredSentimentTargetAdjuster(List<double> Bounds, List<double> Rates) : ISentimentTargetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentTarget item, double valueValue)
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

public sealed record FlaggedSentimentTargetAdjuster(Registry Flags, string Flag, double Percent) : ISentimentTargetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSentimentTargetAdjuster(double Cap) : ISentimentTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSentimentTargetAdjuster(double Step) : ISentimentTargetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SentimentTargetQuantityOffsetAdjuster(double Weight) : ISentimentTargetModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(SentimentTarget item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record TeamSentimentTargetAdjuster(Dictionary<string, double> Factors) : ISentimentTargetModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SentimentTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment target steps in order and keeps a trace of each.</summary>
public sealed class SentimentTargetEngine(List<ISentimentTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentTargetEngine DefaultSentimentTargetEngine(Registry flags)
    {
        return new SentimentTargetEngine(
            new List<ISentimentTargetModifier>
            {
                new TieredSentimentTargetAdjuster(
                    new List<double> { 4931.0, 7964.0 },
                    new List<double> { 0.92, 0.91 }),
                new FlaggedSentimentTargetAdjuster(flags, "sentiment-target-rolling", 1.4),
                new CapSentimentTargetAdjuster(7227.0),
                new RoundingSentimentTargetAdjuster(0.05),
                new SentimentTargetQuantityOffsetAdjuster(1.69),
                new TeamSentimentTargetAdjuster(new Dictionary<string, double> { ["central"] = 0.75 }),
            },
            new List<string>(),
            2.9,
            6562.0);
    }

    public double Price(SentimentTarget item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
