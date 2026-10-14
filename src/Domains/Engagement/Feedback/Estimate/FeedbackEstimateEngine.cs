namespace Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>One step of the feedback estimate calculation.</summary>
public interface IFeedbackEstimateModifier
{
    string Describe();
    double Adjust(FeedbackEstimate item, double valueValue);
}

public sealed record TitleFeedbackEstimateAdjuster(Dictionary<string, double> Factors) : IFeedbackEstimateModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FeedbackEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentFeedbackEstimateAdjuster(double Percent) : IFeedbackEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFeedbackEstimateAdjuster(double Minimum) : IFeedbackEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FeedbackEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFeedbackEstimateAdjuster(double Cap) : IFeedbackEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FeedbackEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the feedback estimate steps in order and keeps a trace of each.</summary>
public sealed class FeedbackEstimateEngine(List<IFeedbackEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackEstimateEngine DefaultFeedbackEstimateEngine()
    {
        return new FeedbackEstimateEngine(
            new List<IFeedbackEstimateModifier>
            {
                new TitleFeedbackEstimateAdjuster(new Dictionary<string, double> { ["north"] = 1.03 }),
                new PercentFeedbackEstimateAdjuster(-8.8),
                new MinimumFeedbackEstimateAdjuster(48.8),
                new CapFeedbackEstimateAdjuster(10605.0),
            },
            new List<string>(),
            1.0,
            86530.7);
    }

    public double Evaluate(FeedbackEstimate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
