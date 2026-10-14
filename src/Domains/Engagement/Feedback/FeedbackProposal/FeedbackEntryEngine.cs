namespace Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>One step of the feedback entry calculation.</summary>
public interface IFeedbackEntryFactor
{
    string Describe();
    double Adjust(FeedbackEntry item, double valueValue);
}

public sealed record CapFeedbackEntryAdjuster(double Cap) : IFeedbackEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentFeedbackEntryAdjuster(double Percent) : IFeedbackEntryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelFeedbackEntryAdjuster(Dictionary<string, double> Factors) : IFeedbackEntryFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FeedbackEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the feedback entry steps in order and keeps a trace of each.</summary>
public sealed class FeedbackEntryEngine(List<IFeedbackEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackEntryEngine DefaultFeedbackEntryEngine()
    {
        return new FeedbackEntryEngine(
            new List<IFeedbackEntryFactor>
            {
                new CapFeedbackEntryAdjuster(17188.0),
                new PercentFeedbackEntryAdjuster(-17.4),
                new LabelFeedbackEntryAdjuster(new Dictionary<string, double> { ["central"] = 1.1 }),
            },
            new List<string>(),
            2.9,
            14900.2);
    }

    public double Evaluate(FeedbackEntry item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
