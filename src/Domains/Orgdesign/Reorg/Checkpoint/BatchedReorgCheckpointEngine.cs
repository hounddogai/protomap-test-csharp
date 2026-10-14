namespace Acme.Workforce.Orgdesign.Reorg.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched reorg checkpoint calculation.</summary>
public interface IBatchedReorgCheckpointFactor
{
    string Describe();
    double Adjust(BatchedReorgCheckpoint item, double valueValue);
}

public sealed record PercentBatchedReorgCheckpointAdjuster(double Percent) : IBatchedReorgCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedReorgCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentBatchedReorgCheckpointAdjuster(Dictionary<string, double> Factors) : IBatchedReorgCheckpointFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BatchedReorgCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BatchedReorgCheckpointDurationMinutesOffsetAdjuster(double Weight) : IBatchedReorgCheckpointFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(BatchedReorgCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record FlaggedBatchedReorgCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IBatchedReorgCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedReorgCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched reorg checkpoint steps in order and keeps a trace of each.</summary>
public sealed class BatchedReorgCheckpointEngine(List<IBatchedReorgCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedReorgCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedReorgCheckpointEngine DefaultBatchedReorgCheckpointEngine(Registry flags)
    {
        return new BatchedReorgCheckpointEngine(
            new List<IBatchedReorgCheckpointFactor>
            {
                new PercentBatchedReorgCheckpointAdjuster(-6.9),
                new SegmentBatchedReorgCheckpointAdjuster(new Dictionary<string, double> { ["retail"] = 0.94 }),
                new BatchedReorgCheckpointDurationMinutesOffsetAdjuster(0.3),
                new FlaggedBatchedReorgCheckpointAdjuster(flags, "batched-reorg-checkpoint-capped", 8.0),
            },
            new List<string>(),
            3.3,
            72711.0);
    }

    public double Evaluate(BatchedReorgCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedReorgCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
