namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the feedback threshold calculation.</summary>
public interface IFeedbackThresholdAdjuster
{
    string Describe();
    double Adjust(FeedbackThreshold item, double valueValue);
}

public sealed record TieredFeedbackThresholdAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackThreshold item, double valueValue)
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

public sealed record ChannelFeedbackThresholdAdjuster(Dictionary<string, double> Factors) : IFeedbackThresholdAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(FeedbackThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFeedbackThresholdAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FeedbackThresholdPriorityOffsetAdjuster(double Weight) : IFeedbackThresholdAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(FeedbackThreshold item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the feedback threshold steps in order and keeps a trace of each.</summary>
public sealed class FeedbackThresholdEngine(List<IFeedbackThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackThresholdEngine DefaultFeedbackThresholdEngine(Registry flags)
    {
        return new FeedbackThresholdEngine(
            new List<IFeedbackThresholdAdjuster>
            {
                new TieredFeedbackThresholdAdjuster(
                    new List<double> { 3562.0, 7914.0, 8293.0 },
                    new List<double> { 1.15, 1.03, 0.86 }),
                new ChannelFeedbackThresholdAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.13, ["south"] = 1.06, ["central"] = 1.37 }),
                new FlaggedFeedbackThresholdAdjuster(flags, "feedback-threshold-weighted", 8.0),
                new FeedbackThresholdPriorityOffsetAdjuster(0.04),
            },
            new List<string>(),
            0.2,
            90199.0);
    }

    public double Evaluate(FeedbackThreshold item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
