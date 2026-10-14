namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched accrual checkpoint calculation.</summary>
public interface IBatchedAccrualCheckpointModifier
{
    string Describe();
    double Adjust(BatchedAccrualCheckpoint item, double valueValue);
}

public sealed record SourceBatchedAccrualCheckpointAdjuster(Dictionary<string, double> Factors) : IBatchedAccrualCheckpointModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(BatchedAccrualCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedAccrualCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAccrualCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAccrualCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedAccrualCheckpointAdjuster(double Cap) : IBatchedAccrualCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedAccrualCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BatchedAccrualCheckpointThresholdOffsetAdjuster(double Weight) : IBatchedAccrualCheckpointModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(BatchedAccrualCheckpoint item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record MinimumBatchedAccrualCheckpointAdjuster(double Minimum) : IBatchedAccrualCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedAccrualCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredBatchedAccrualCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IBatchedAccrualCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedAccrualCheckpoint item, double valueValue)
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

/// <summary>Runs the batched accrual checkpoint steps in order and keeps a trace of each.</summary>
public sealed class BatchedAccrualCheckpointEngine(List<IBatchedAccrualCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAccrualCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAccrualCheckpointEngine DefaultBatchedAccrualCheckpointEngine(Registry flags)
    {
        return new BatchedAccrualCheckpointEngine(
            new List<IBatchedAccrualCheckpointModifier>
            {
                new SourceBatchedAccrualCheckpointAdjuster(new Dictionary<string, double> { ["south"] = 0.98 }),
                new FlaggedBatchedAccrualCheckpointAdjuster(flags, "batched-accrual-checkpoint-capped", -2.2),
                new CapBatchedAccrualCheckpointAdjuster(30523.0),
                new BatchedAccrualCheckpointThresholdOffsetAdjuster(0.32),
                new MinimumBatchedAccrualCheckpointAdjuster(48.3),
                new TieredBatchedAccrualCheckpointAdjuster(
                    new List<double> { 2963.0, 4022.0 },
                    new List<double> { 1.11, 0.85 }),
            },
            new List<string>(),
            0.4,
            62465.1);
    }

    public double Compute(BatchedAccrualCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAccrualCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
