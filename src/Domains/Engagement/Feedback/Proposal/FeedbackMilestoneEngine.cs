namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback milestone calculation.</summary>
public interface IFeedbackMilestoneModifier
{
    string Describe();
    double Adjust(FeedbackMilestone item, double valueValue);
}

public sealed record TieredFeedbackMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IFeedbackMilestoneModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FeedbackMilestone item, double valueValue)
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

public sealed record RoundingFeedbackMilestoneAdjuster(double Step) : IFeedbackMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFeedbackMilestoneAdjuster(double Cap) : IFeedbackMilestoneModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedFeedbackMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FeedbackMilestoneLimitOffsetAdjuster(double Weight) : IFeedbackMilestoneModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(FeedbackMilestone item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentFeedbackMilestoneAdjuster(double Percent) : IFeedbackMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the feedback milestone steps in order and keeps a trace of each.</summary>
public sealed class FeedbackMilestoneEngine(List<IFeedbackMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackMilestoneEngine DefaultFeedbackMilestoneEngine(Registry flags)
    {
        return new FeedbackMilestoneEngine(
            new List<IFeedbackMilestoneModifier>
            {
                new TieredFeedbackMilestoneAdjuster(
                    new List<double> { 550.0, 8117.0, 8693.0 },
                    new List<double> { 1.04, 1.19, 0.89 }),
                new RoundingFeedbackMilestoneAdjuster(0.25),
                new CapFeedbackMilestoneAdjuster(19236.0),
                new FlaggedFeedbackMilestoneAdjuster(flags, "feedback-milestone-tiered", -4.7),
                new FeedbackMilestoneLimitOffsetAdjuster(1.44),
                new PercentFeedbackMilestoneAdjuster(14.5),
            },
            new List<string>(),
            5.0,
            93146.7);
    }

    public double Evaluate(FeedbackMilestone item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
