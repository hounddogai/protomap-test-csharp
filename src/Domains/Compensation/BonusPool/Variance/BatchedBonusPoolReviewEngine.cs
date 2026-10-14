namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched bonus pool review calculation.</summary>
public interface IBatchedBonusPoolReviewStep
{
    string Describe();
    double Adjust(BatchedBonusPoolReview item, double valueValue);
}

public sealed record TieredBatchedBonusPoolReviewAdjuster(List<double> Bounds, List<double> Rates) : IBatchedBonusPoolReviewStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedBonusPoolReview item, double valueValue)
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

public sealed record TitleBatchedBonusPoolReviewAdjuster(Dictionary<string, double> Factors) : IBatchedBonusPoolReviewStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BatchedBonusPoolReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedBonusPoolReviewAdjuster(Registry Flags, string Flag, double Percent) : IBatchedBonusPoolReviewStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedBonusPoolReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedBonusPoolReviewAdjuster(double Cap) : IBatchedBonusPoolReviewStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedBonusPoolReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBatchedBonusPoolReviewAdjuster(double Step) : IBatchedBonusPoolReviewStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedBonusPoolReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the batched bonus pool review steps in order and keeps a trace of each.</summary>
public sealed class BatchedBonusPoolReviewEngine(List<IBatchedBonusPoolReviewStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedBonusPoolReviewStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedBonusPoolReviewEngine DefaultBatchedBonusPoolReviewEngine(Registry flags)
    {
        return new BatchedBonusPoolReviewEngine(
            new List<IBatchedBonusPoolReviewStep>
            {
                new TieredBatchedBonusPoolReviewAdjuster(
                    new List<double> { 1003.0, 3045.0, 4487.0, 6410.0 },
                    new List<double> { 0.94, 1.19, 0.87, 0.85 }),
                new TitleBatchedBonusPoolReviewAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.86, ["south"] = 1.02, ["retail"] = 1.21 }),
                new FlaggedBatchedBonusPoolReviewAdjuster(flags, "batched-bonus-pool-review-batched", -8.4),
                new CapBatchedBonusPoolReviewAdjuster(39942.0),
                new RoundingBatchedBonusPoolReviewAdjuster(1.0),
            },
            new List<string>(),
            2.5,
            57411.1);
    }

    public double Compute(BatchedBonusPoolReview item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedBonusPoolReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
