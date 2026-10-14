namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback review calculation.</summary>
public interface IFeedbackReviewAdjuster
{
    string Describe();
    double Adjust(FeedbackReview item, double valueValue);
}

public sealed record PercentFeedbackReviewAdjuster(double Percent) : IFeedbackReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFeedbackReviewAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFeedbackReviewAdjuster(double Step) : IFeedbackReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the feedback review steps in order and keeps a trace of each.</summary>
public sealed class FeedbackReviewEngine(List<IFeedbackReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackReviewEngine DefaultFeedbackReviewEngine(Registry flags)
    {
        return new FeedbackReviewEngine(
            new List<IFeedbackReviewAdjuster>
            {
                new PercentFeedbackReviewAdjuster(-24.8),
                new FlaggedFeedbackReviewAdjuster(flags, "feedback-review-capped", 7.8),
                new RoundingFeedbackReviewAdjuster(0.05),
            },
            new List<string>(),
            3.1,
            84809.0);
    }

    public double Evaluate(FeedbackReview item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
