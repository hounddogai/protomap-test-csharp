namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped sentiment manifest calculation.</summary>
public interface ICappedSentimentManifestModifier
{
    string Describe();
    double Adjust(CappedSentimentManifest item, double valueValue);
}

public sealed record CapCappedSentimentManifestAdjuster(double Cap) : ICappedSentimentManifestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedSentimentManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedSentimentManifestBatchSizeOffsetAdjuster(double Weight) : ICappedSentimentManifestModifier
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(CappedSentimentManifest item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record MinimumCappedSentimentManifestAdjuster(double Minimum) : ICappedSentimentManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedSentimentManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCappedSentimentManifestAdjuster(Registry Flags, string Flag, double Percent) : ICappedSentimentManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedSentimentManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelCappedSentimentManifestAdjuster(Dictionary<string, double> Factors) : ICappedSentimentManifestModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CappedSentimentManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCappedSentimentManifestAdjuster(double Percent) : ICappedSentimentManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedSentimentManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped sentiment manifest steps in order and keeps a trace of each.</summary>
public sealed class CappedSentimentManifestEngine(List<ICappedSentimentManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedSentimentManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedSentimentManifestEngine DefaultCappedSentimentManifestEngine(Registry flags)
    {
        return new CappedSentimentManifestEngine(
            new List<ICappedSentimentManifestModifier>
            {
                new CapCappedSentimentManifestAdjuster(45885.0),
                new CappedSentimentManifestBatchSizeOffsetAdjuster(1.29),
                new MinimumCappedSentimentManifestAdjuster(20.0),
                new FlaggedCappedSentimentManifestAdjuster(flags, "capped-sentiment-manifest-default", -2.8),
                new ChannelCappedSentimentManifestAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.35, ["north"] = 1.18, ["south"] = 1.28 }),
                new PercentCappedSentimentManifestAdjuster(22.5),
            },
            new List<string>(),
            4.2,
            86505.4);
    }

    public double Compute(CappedSentimentManifest item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedSentimentManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
