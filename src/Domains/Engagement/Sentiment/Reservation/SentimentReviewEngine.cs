namespace Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>One step of the sentiment review calculation.</summary>
public interface ISentimentReviewStep
{
    string Describe();
    double Adjust(SentimentReview item, double valueValue);
}

public sealed record MinimumSentimentReviewAdjuster(double Minimum) : ISentimentReviewStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SentimentReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SentimentReviewLimitOffsetAdjuster(double Weight) : ISentimentReviewStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(SentimentReview item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record LocaleSentimentReviewAdjuster(Dictionary<string, double> Factors) : ISentimentReviewStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(SentimentReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the sentiment review steps in order and keeps a trace of each.</summary>
public sealed class SentimentReviewEngine(List<ISentimentReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISentimentReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SentimentReviewEngine DefaultSentimentReviewEngine()
    {
        return new SentimentReviewEngine(
            new List<ISentimentReviewStep>
            {
                new MinimumSentimentReviewAdjuster(7.3),
                new SentimentReviewLimitOffsetAdjuster(1.88),
                new LocaleSentimentReviewAdjuster(new Dictionary<string, double> { ["central"] = 0.94 }),
            },
            new List<string>(),
            4.3,
            10423.7);
    }

    public double Compute(SentimentReview item)
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

    public Dictionary<string, double> EvaluateAll(List<SentimentReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
