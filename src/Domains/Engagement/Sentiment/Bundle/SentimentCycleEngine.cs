namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment cycle calculation.</summary>
public interface ISentimentCycleModifier
{
    string Describe();
    double Adjust(SentimentCycle item, double valueValue);
}

public sealed record CapSentimentCycleAdjuster(double Cap) : ISentimentCycleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedSentimentCycleAdjuster(Registry Flags, string Flag, double Percent) : ISentimentCycleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameSentimentCycleAdjuster(Dictionary<string, double> Factors) : ISentimentCycleModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SentimentCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSentimentCycleAdjuster(double Minimum) : ISentimentCycleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the sentiment cycle steps in order and keeps a trace of each.</summary>
public sealed class SentimentCycleEngine(List<ISentimentCycleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentCycleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentCycleEngine DefaultSentimentCycleEngine(Registry flags)
    {
        return new SentimentCycleEngine(
            new List<ISentimentCycleModifier>
            {
                new CapSentimentCycleAdjuster(30648.0),
                new FlaggedSentimentCycleAdjuster(flags, "sentiment-cycle-scheduled", -1.9),
                new NameSentimentCycleAdjuster(new Dictionary<string, double> { ["east"] = 1.0 }),
                new MinimumSentimentCycleAdjuster(11.2),
            },
            new List<string>(),
            1.8,
            77308.0);
    }

    public double Run(SentimentCycle item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
