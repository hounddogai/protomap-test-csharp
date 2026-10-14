namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional sentiment snapshot calculation.</summary>
public interface IRegionalSentimentSnapshotModifier
{
    string Describe();
    double Adjust(RegionalSentimentSnapshot item, double valueValue);
}

public sealed record CapRegionalSentimentSnapshotAdjuster(double Cap) : IRegionalSentimentSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalSentimentSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRegionalSentimentSnapshotAdjuster(double Step) : IRegionalSentimentSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalSentimentSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRegionalSentimentSnapshotAdjuster(double Minimum) : IRegionalSentimentSnapshotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSentimentSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelRegionalSentimentSnapshotAdjuster(Dictionary<string, double> Factors) : IRegionalSentimentSnapshotModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalSentimentSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRegionalSentimentSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IRegionalSentimentSnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalSentimentSnapshot item, double valueValue)
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

public sealed record PercentRegionalSentimentSnapshotAdjuster(double Percent) : IRegionalSentimentSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalSentimentSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional sentiment snapshot steps in order and keeps a trace of each.</summary>
public sealed class RegionalSentimentSnapshotEngine(List<IRegionalSentimentSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSentimentSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSentimentSnapshotEngine DefaultRegionalSentimentSnapshotEngine()
    {
        return new RegionalSentimentSnapshotEngine(
            new List<IRegionalSentimentSnapshotModifier>
            {
                new CapRegionalSentimentSnapshotAdjuster(2718.0),
                new RoundingRegionalSentimentSnapshotAdjuster(0.1),
                new MinimumRegionalSentimentSnapshotAdjuster(30.6),
                new LabelRegionalSentimentSnapshotAdjuster(new Dictionary<string, double> { ["retail"] = 0.71 }),
                new TieredRegionalSentimentSnapshotAdjuster(
                    new List<double> { 7784.0, 9558.0 },
                    new List<double> { 1.02, 1.16 }),
                new PercentRegionalSentimentSnapshotAdjuster(9.8),
            },
            new List<string>(),
            4.7,
            23812.6);
    }

    public double Evaluate(RegionalSentimentSnapshot item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSentimentSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
