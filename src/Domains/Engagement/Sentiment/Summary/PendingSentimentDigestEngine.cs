namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending sentiment digest calculation.</summary>
public interface IPendingSentimentDigestAdjuster
{
    string Describe();
    double Adjust(PendingSentimentDigest item, double valueValue);
}

public sealed record FlaggedPendingSentimentDigestAdjuster(Registry Flags, string Flag, double Percent) : IPendingSentimentDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingSentimentDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingSentimentDigestScoreOffsetAdjuster(double Weight) : IPendingSentimentDigestAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(PendingSentimentDigest item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record CapPendingSentimentDigestAdjuster(double Cap) : IPendingSentimentDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingSentimentDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingSentimentDigestAdjuster(List<double> Bounds, List<double> Rates) : IPendingSentimentDigestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingSentimentDigest item, double valueValue)
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

public sealed record SegmentPendingSentimentDigestAdjuster(Dictionary<string, double> Factors) : IPendingSentimentDigestAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(PendingSentimentDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingSentimentDigestAdjuster(double Minimum) : IPendingSentimentDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingSentimentDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending sentiment digest steps in order and keeps a trace of each.</summary>
public sealed class PendingSentimentDigestEngine(List<IPendingSentimentDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingSentimentDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingSentimentDigestEngine DefaultPendingSentimentDigestEngine(Registry flags)
    {
        return new PendingSentimentDigestEngine(
            new List<IPendingSentimentDigestAdjuster>
            {
                new FlaggedPendingSentimentDigestAdjuster(flags, "pending-sentiment-digest-primary", 3.5),
                new PendingSentimentDigestScoreOffsetAdjuster(0.42),
                new CapPendingSentimentDigestAdjuster(43289.0),
                new TieredPendingSentimentDigestAdjuster(
                    new List<double> { 4947.0, 7237.0, 7722.0 },
                    new List<double> { 0.93, 0.97, 0.92 }),
                new SegmentPendingSentimentDigestAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.32, ["online"] = 1.13, ["central"] = 1.14 }),
                new MinimumPendingSentimentDigestAdjuster(43.9),
            },
            new List<string>(),
            0.8,
            36299.3);
    }

    public double Evaluate(PendingSentimentDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingSentimentDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
