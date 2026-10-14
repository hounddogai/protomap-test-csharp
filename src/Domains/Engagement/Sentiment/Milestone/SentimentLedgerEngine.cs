namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment ledger calculation.</summary>
public interface ISentimentLedgerModifier
{
    string Describe();
    double Adjust(SentimentLedger item, double valueValue);
}

public sealed record SentimentLedgerCostOffsetAdjuster(double Weight) : ISentimentLedgerModifier
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(SentimentLedger item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record PercentSentimentLedgerAdjuster(double Percent) : ISentimentLedgerModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SentimentLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSentimentLedgerAdjuster(double Step) : ISentimentLedgerModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the sentiment ledger steps in order and keeps a trace of each.</summary>
public sealed class SentimentLedgerEngine(List<ISentimentLedgerModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentLedgerModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentLedgerEngine DefaultSentimentLedgerEngine()
    {
        return new SentimentLedgerEngine(
            new List<ISentimentLedgerModifier>
            {
                new SentimentLedgerCostOffsetAdjuster(1.7),
                new PercentSentimentLedgerAdjuster(-13.1),
                new RoundingSentimentLedgerAdjuster(0.5),
            },
            new List<string>(),
            1.3,
            84950.8);
    }

    public double Compute(SentimentLedger item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
