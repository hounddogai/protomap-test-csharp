namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment exception calculation.</summary>
public interface ISentimentExceptionAdjuster
{
    string Describe();
    double Adjust(SentimentException item, double valueValue);
}

public sealed record PercentSentimentExceptionAdjuster(double Percent) : ISentimentExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SentimentExceptionMaximumOffsetAdjuster(double Weight) : ISentimentExceptionAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(SentimentException item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record TieredSentimentExceptionAdjuster(List<double> Bounds, List<double> Rates) : ISentimentExceptionAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentException item, double valueValue)
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

public sealed record RoundingSentimentExceptionAdjuster(double Step) : ISentimentExceptionAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSentimentExceptionAdjuster(Registry Flags, string Flag, double Percent) : ISentimentExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelSentimentExceptionAdjuster(Dictionary<string, double> Factors) : ISentimentExceptionAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SentimentException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment exception steps in order and keeps a trace of each.</summary>
public sealed class SentimentExceptionEngine(List<ISentimentExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentExceptionEngine DefaultSentimentExceptionEngine(Registry flags)
    {
        return new SentimentExceptionEngine(
            new List<ISentimentExceptionAdjuster>
            {
                new PercentSentimentExceptionAdjuster(-12.6),
                new SentimentExceptionMaximumOffsetAdjuster(0.06),
                new TieredSentimentExceptionAdjuster(
                    new List<double> { 1353.0, 5043.0, 5274.0, 9978.0 },
                    new List<double> { 1.01, 0.89, 0.83, 0.92 }),
                new RoundingSentimentExceptionAdjuster(0.1),
                new FlaggedSentimentExceptionAdjuster(flags, "sentiment-exception-lenient", -4.7),
                new LabelSentimentExceptionAdjuster(new Dictionary<string, double> { ["retail"] = 1.21 }),
            },
            new List<string>(),
            2.5,
            2729.4);
    }

    public double Evaluate(SentimentException item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
