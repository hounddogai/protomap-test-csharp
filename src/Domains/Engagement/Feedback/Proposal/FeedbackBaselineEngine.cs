namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback baseline calculation.</summary>
public interface IFeedbackBaselineAdjuster
{
    string Describe();
    double Adjust(FeedbackBaseline item, double valueValue);
}

public sealed record PercentFeedbackBaselineAdjuster(double Percent) : IFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFeedbackBaselineAdjuster(Registry Flags, string Flag, double Percent) : IFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FeedbackBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFeedbackBaselineAdjuster(double Step) : IFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FeedbackBaselineVersionOffsetAdjuster(double Weight) : IFeedbackBaselineAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(FeedbackBaseline item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the feedback baseline steps in order and keeps a trace of each.</summary>
public sealed class FeedbackBaselineEngine(List<IFeedbackBaselineAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackBaselineAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackBaselineEngine DefaultFeedbackBaselineEngine(Registry flags)
    {
        return new FeedbackBaselineEngine(
            new List<IFeedbackBaselineAdjuster>
            {
                new PercentFeedbackBaselineAdjuster(11.5),
                new FlaggedFeedbackBaselineAdjuster(flags, "feedback-baseline-lenient", -7.3),
                new RoundingFeedbackBaselineAdjuster(0.25),
                new FeedbackBaselineVersionOffsetAdjuster(0.1),
            },
            new List<string>(),
            2.0,
            10989.2);
    }

    public double Compute(FeedbackBaseline item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
