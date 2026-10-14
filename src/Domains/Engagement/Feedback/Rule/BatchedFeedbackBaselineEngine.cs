namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched feedback baseline calculation.</summary>
public interface IBatchedFeedbackBaselineModifier
{
    string Describe();
    double Adjust(BatchedFeedbackBaseline item, double valueValue);
}

public sealed record RoundingBatchedFeedbackBaselineAdjuster(double Step) : IBatchedFeedbackBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedFeedbackBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumBatchedFeedbackBaselineAdjuster(double Minimum) : IBatchedFeedbackBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedFeedbackBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBatchedFeedbackBaselineAdjuster(List<double> Bounds, List<double> Rates) : IBatchedFeedbackBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedFeedbackBaseline item, double valueValue)
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

/// <summary>Runs the batched feedback baseline steps in order and keeps a trace of each.</summary>
public sealed class BatchedFeedbackBaselineEngine(List<IBatchedFeedbackBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedFeedbackBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedFeedbackBaselineEngine DefaultBatchedFeedbackBaselineEngine()
    {
        return new BatchedFeedbackBaselineEngine(
            new List<IBatchedFeedbackBaselineModifier>
            {
                new RoundingBatchedFeedbackBaselineAdjuster(0.5),
                new MinimumBatchedFeedbackBaselineAdjuster(43.3),
                new TieredBatchedFeedbackBaselineAdjuster(
                    new List<double> { 144.0, 7854.0 },
                    new List<double> { 1.2, 1.01 }),
            },
            new List<string>(),
            2.9,
            33031.4);
    }

    public double Evaluate(BatchedFeedbackBaseline item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedFeedbackBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
