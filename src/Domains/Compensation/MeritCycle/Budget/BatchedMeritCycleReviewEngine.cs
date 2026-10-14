namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched merit cycle review calculation.</summary>
public interface IBatchedMeritCycleReviewFactor
{
    string Describe();
    double Adjust(BatchedMeritCycleReview item, double valueValue);
}

public sealed record FlaggedBatchedMeritCycleReviewAdjuster(Registry Flags, string Flag, double Percent) : IBatchedMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedMeritCycleReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedMeritCycleReviewTenureMonthsOffsetAdjuster(double Weight) : IBatchedMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(BatchedMeritCycleReview item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record CapBatchedMeritCycleReviewAdjuster(double Cap) : IBatchedMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedMeritCycleReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBatchedMeritCycleReviewAdjuster(double Step) : IBatchedMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedMeritCycleReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredBatchedMeritCycleReviewAdjuster(List<double> Bounds, List<double> Rates) : IBatchedMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedMeritCycleReview item, double valueValue)
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

public sealed record PercentBatchedMeritCycleReviewAdjuster(double Percent) : IBatchedMeritCycleReviewFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedMeritCycleReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched merit cycle review steps in order and keeps a trace of each.</summary>
public sealed class BatchedMeritCycleReviewEngine(List<IBatchedMeritCycleReviewFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedMeritCycleReviewFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedMeritCycleReviewEngine DefaultBatchedMeritCycleReviewEngine(Registry flags)
    {
        return new BatchedMeritCycleReviewEngine(
            new List<IBatchedMeritCycleReviewFactor>
            {
                new FlaggedBatchedMeritCycleReviewAdjuster(flags, "batched-merit-cycle-review-pending", -3.5),
                new BatchedMeritCycleReviewTenureMonthsOffsetAdjuster(0.06),
                new CapBatchedMeritCycleReviewAdjuster(15558.0),
                new RoundingBatchedMeritCycleReviewAdjuster(0.25),
                new TieredBatchedMeritCycleReviewAdjuster(
                    new List<double> { 7310.0, 7392.0, 8659.0 },
                    new List<double> { 1.14, 0.92, 0.82 }),
                new PercentBatchedMeritCycleReviewAdjuster(18.8),
            },
            new List<string>(),
            3.2,
            22963.2);
    }

    public double Compute(BatchedMeritCycleReview item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedMeritCycleReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
