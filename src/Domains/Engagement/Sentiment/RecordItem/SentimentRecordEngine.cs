namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment record calculation.</summary>
public interface ISentimentRecordAdjuster
{
    string Describe();
    double Adjust(SentimentRecord item, double valueValue);
}

public sealed record RoundingSentimentRecordAdjuster(double Step) : ISentimentRecordAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSentimentRecordAdjuster(Registry Flags, string Flag, double Percent) : ISentimentRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSentimentRecordAdjuster(double Percent) : ISentimentRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSentimentRecordAdjuster(double Cap) : ISentimentRecordAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SentimentRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SentimentRecordPriorityOffsetAdjuster(double Weight) : ISentimentRecordAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(SentimentRecord item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record TieredSentimentRecordAdjuster(List<double> Bounds, List<double> Rates) : ISentimentRecordAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentRecord item, double valueValue)
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

/// <summary>Runs the sentiment record steps in order and keeps a trace of each.</summary>
public sealed class SentimentRecordEngine(List<ISentimentRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentRecordEngine DefaultSentimentRecordEngine(Registry flags)
    {
        return new SentimentRecordEngine(
            new List<ISentimentRecordAdjuster>
            {
                new RoundingSentimentRecordAdjuster(0.25),
                new FlaggedSentimentRecordAdjuster(flags, "sentiment-record-scheduled", -7.6),
                new PercentSentimentRecordAdjuster(-1.9),
                new CapSentimentRecordAdjuster(47339.0),
                new SentimentRecordPriorityOffsetAdjuster(1.11),
                new TieredSentimentRecordAdjuster(
                    new List<double> { 4102.0, 9468.0 },
                    new List<double> { 1.04, 1.0 }),
            },
            new List<string>(),
            1.0,
            71012.6);
    }

    public double Evaluate(SentimentRecord item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
