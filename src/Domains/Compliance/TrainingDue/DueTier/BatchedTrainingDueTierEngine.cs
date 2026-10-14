namespace Acme.Workforce.Compliance.TrainingDue.DueTier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched training due tier calculation.</summary>
public interface IBatchedTrainingDueTierStep
{
    string Describe();
    double Adjust(BatchedTrainingDueTier item, double valueValue);
}

public sealed record BatchedTrainingDueTierEndsAtOffsetAdjuster(double Weight) : IBatchedTrainingDueTierStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(BatchedTrainingDueTier item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record FlaggedBatchedTrainingDueTierAdjuster(Registry Flags, string Flag, double Percent) : IBatchedTrainingDueTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedTrainingDueTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedTrainingDueTierAdjuster(List<double> Bounds, List<double> Rates) : IBatchedTrainingDueTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedTrainingDueTier item, double valueValue)
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

public sealed record CapBatchedTrainingDueTierAdjuster(double Cap) : IBatchedTrainingDueTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedTrainingDueTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBatchedTrainingDueTierAdjuster(double Step) : IBatchedTrainingDueTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedTrainingDueTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBatchedTrainingDueTierAdjuster(double Percent) : IBatchedTrainingDueTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedTrainingDueTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched training due tier steps in order and keeps a trace of each.</summary>
public sealed class BatchedTrainingDueTierEngine(List<IBatchedTrainingDueTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedTrainingDueTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedTrainingDueTierEngine DefaultBatchedTrainingDueTierEngine(Registry flags)
    {
        return new BatchedTrainingDueTierEngine(
            new List<IBatchedTrainingDueTierStep>
            {
                new BatchedTrainingDueTierEndsAtOffsetAdjuster(0.6),
                new FlaggedBatchedTrainingDueTierAdjuster(flags, "batched-training-due-tier-legacy", -1.8),
                new TieredBatchedTrainingDueTierAdjuster(
                    new List<double> { 3687.0, 5788.0, 7533.0, 9140.0 },
                    new List<double> { 0.81, 0.95, 1.11, 0.88 }),
                new CapBatchedTrainingDueTierAdjuster(36925.0),
                new RoundingBatchedTrainingDueTierAdjuster(1.0),
                new PercentBatchedTrainingDueTierAdjuster(-18.1),
            },
            new List<string>(),
            4.8,
            71921.8);
    }

    public double Compute(BatchedTrainingDueTier item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedTrainingDueTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
