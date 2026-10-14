namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback quota calculation.</summary>
public interface IFeedbackQuotaModifier
{
    string Describe();
    double Adjust(FeedbackQuota item, double valueValue);
}

public sealed record RegionFeedbackQuotaAdjuster(Dictionary<string, double> Factors) : IFeedbackQuotaModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(FeedbackQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFeedbackQuotaAdjuster(double Step) : IFeedbackQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFeedbackQuotaAdjuster(double Minimum) : IFeedbackQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FeedbackQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFeedbackQuotaAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredFeedbackQuotaAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackQuota item, double valueValue)
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

public sealed record PercentFeedbackQuotaAdjuster(double Percent) : IFeedbackQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the feedback quota steps in order and keeps a trace of each.</summary>
public sealed class FeedbackQuotaEngine(List<IFeedbackQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackQuotaEngine DefaultFeedbackQuotaEngine(Registry flags)
    {
        return new FeedbackQuotaEngine(
            new List<IFeedbackQuotaModifier>
            {
                new RegionFeedbackQuotaAdjuster(new Dictionary<string, double> { ["east"] = 1.16 }),
                new RoundingFeedbackQuotaAdjuster(0.5),
                new MinimumFeedbackQuotaAdjuster(6.5),
                new FlaggedFeedbackQuotaAdjuster(flags, "feedback-quota-regional", 8.6),
                new TieredFeedbackQuotaAdjuster(
                    new List<double> { 257.0, 2301.0, 3165.0, 7651.0 },
                    new List<double> { 0.94, 0.88, 0.99, 0.93 }),
                new PercentFeedbackQuotaAdjuster(25.0),
            },
            new List<string>(),
            1.4,
            31755.2);
    }

    public double Compute(FeedbackQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
