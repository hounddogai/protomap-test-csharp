namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched bonus pool threshold calculation.</summary>
public interface IBatchedBonusPoolThresholdStep
{
    string Describe();
    double Adjust(BatchedBonusPoolThreshold item, double valueValue);
}

public sealed record RoundingBatchedBonusPoolThresholdAdjuster(double Step) : IBatchedBonusPoolThresholdStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedBonusPoolThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBatchedBonusPoolThresholdAdjuster(Registry Flags, string Flag, double Percent) : IBatchedBonusPoolThresholdStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedBonusPoolThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBatchedBonusPoolThresholdAdjuster(double Percent) : IBatchedBonusPoolThresholdStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedBonusPoolThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedBonusPoolThresholdAttemptsOffsetAdjuster(double Weight) : IBatchedBonusPoolThresholdStep
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(BatchedBonusPoolThreshold item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record UnitBatchedBonusPoolThresholdAdjuster(Dictionary<string, double> Factors) : IBatchedBonusPoolThresholdStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(BatchedBonusPoolThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched bonus pool threshold steps in order and keeps a trace of each.</summary>
public sealed class BatchedBonusPoolThresholdEngine(List<IBatchedBonusPoolThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedBonusPoolThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedBonusPoolThresholdEngine DefaultBatchedBonusPoolThresholdEngine(Registry flags)
    {
        return new BatchedBonusPoolThresholdEngine(
            new List<IBatchedBonusPoolThresholdStep>
            {
                new RoundingBatchedBonusPoolThresholdAdjuster(0.5),
                new FlaggedBatchedBonusPoolThresholdAdjuster(flags, "batched-bonus-pool-threshold-primary", 0.3),
                new PercentBatchedBonusPoolThresholdAdjuster(-11.2),
                new BatchedBonusPoolThresholdAttemptsOffsetAdjuster(1.88),
                new UnitBatchedBonusPoolThresholdAdjuster(new Dictionary<string, double> { ["east"] = 1.34 }),
            },
            new List<string>(),
            4.5,
            91638.9);
    }

    public double Compute(BatchedBonusPoolThreshold item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedBonusPoolThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
