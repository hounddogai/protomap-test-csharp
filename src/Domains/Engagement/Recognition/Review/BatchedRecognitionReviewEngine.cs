namespace Acme.Workforce.Engagement.Recognition.Review;

/// <summary>One step of the batched recognition review calculation.</summary>
public interface IBatchedRecognitionReviewFactor
{
    string Describe();
    double Adjust(BatchedRecognitionReview item, double valueValue);
}

public sealed record MinimumBatchedRecognitionReviewAdjuster(double Minimum) : IBatchedRecognitionReviewFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedRecognitionReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBatchedRecognitionReviewAdjuster(List<double> Bounds, List<double> Rates) : IBatchedRecognitionReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedRecognitionReview item, double valueValue)
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

public sealed record PercentBatchedRecognitionReviewAdjuster(double Percent) : IBatchedRecognitionReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedRecognitionReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameBatchedRecognitionReviewAdjuster(Dictionary<string, double> Factors) : IBatchedRecognitionReviewFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BatchedRecognitionReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched recognition review steps in order and keeps a trace of each.</summary>
public sealed class BatchedRecognitionReviewEngine(List<IBatchedRecognitionReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedRecognitionReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedRecognitionReviewEngine DefaultBatchedRecognitionReviewEngine()
    {
        return new BatchedRecognitionReviewEngine(
            new List<IBatchedRecognitionReviewFactor>
            {
                new MinimumBatchedRecognitionReviewAdjuster(26.2),
                new TieredBatchedRecognitionReviewAdjuster(
                    new List<double> { 1734.0, 7597.0 },
                    new List<double> { 0.97, 0.95 }),
                new PercentBatchedRecognitionReviewAdjuster(-15.7),
                new NameBatchedRecognitionReviewAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.8, ["west"] = 0.87, ["south"] = 1.16 }),
            },
            new List<string>(),
            4.8,
            44195.1);
    }

    public double Run(BatchedRecognitionReview item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedRecognitionReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
