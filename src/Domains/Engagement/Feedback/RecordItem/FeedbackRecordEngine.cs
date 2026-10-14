namespace Acme.Workforce.Engagement.Feedback.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the feedback record calculation.</summary>
public interface IFeedbackRecordStep
{
    string Describe();
    double Adjust(FeedbackRecord item, double valueValue);
}

public sealed record RoundingFeedbackRecordAdjuster(double Step) : IFeedbackRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FeedbackRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFeedbackRecordAdjuster(double Minimum) : IFeedbackRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FeedbackRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFeedbackRecordAdjuster(double Percent) : IFeedbackRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FeedbackRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the feedback record steps in order and keeps a trace of each.</summary>
public sealed class FeedbackRecordEngine(List<IFeedbackRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFeedbackRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FeedbackRecordEngine DefaultFeedbackRecordEngine()
    {
        return new FeedbackRecordEngine(
            new List<IFeedbackRecordStep>
            {
                new RoundingFeedbackRecordAdjuster(1.0),
                new MinimumFeedbackRecordAdjuster(9.7),
                new PercentFeedbackRecordAdjuster(22.0),
            },
            new List<string>(),
            2.7,
            53328.0);
    }

    public double Price(FeedbackRecord item)
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

    public Dictionary<string, double> EvaluateAll(List<FeedbackRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
