namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the sentiment snapshot calculation.</summary>
public interface ISentimentSnapshotStep
{
    string Describe();
    double Adjust(SentimentSnapshot item, double valueValue);
}

public sealed record MinimumSentimentSnapshotAdjuster(double Minimum) : ISentimentSnapshotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSentimentSnapshotAdjuster(double Step) : ISentimentSnapshotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SentimentSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSentimentSnapshotAdjuster(List<double> Bounds, List<double> Rates) : ISentimentSnapshotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SentimentSnapshot item, double valueValue)
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

public sealed record FlaggedSentimentSnapshotAdjuster(Registry Flags, string Flag, double Percent) : ISentimentSnapshotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SentimentSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the sentiment snapshot steps in order and keeps a trace of each.</summary>
public sealed class SentimentSnapshotEngine(List<ISentimentSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentSnapshotEngine DefaultSentimentSnapshotEngine(Registry flags)
    {
        return new SentimentSnapshotEngine(
            new List<ISentimentSnapshotStep>
            {
                new MinimumSentimentSnapshotAdjuster(25.3),
                new RoundingSentimentSnapshotAdjuster(1.0),
                new TieredSentimentSnapshotAdjuster(
                    new List<double> { 443.0, 3661.0, 5010.0, 8772.0 },
                    new List<double> { 1.02, 1.09, 1.19, 1.16 }),
                new FlaggedSentimentSnapshotAdjuster(flags, "sentiment-snapshot-manual", -7.0),
            },
            new List<string>(),
            4.2,
            33077.6);
    }

    public double Run(SentimentSnapshot item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SentimentSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
