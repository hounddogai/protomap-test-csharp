namespace Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>One step of the batched policy ack snapshot calculation.</summary>
public interface IBatchedPolicyAckSnapshotStep
{
    string Describe();
    double Adjust(BatchedPolicyAckSnapshot item, double valueValue);
}

public sealed record TitleBatchedPolicyAckSnapshotAdjuster(Dictionary<string, double> Factors) : IBatchedPolicyAckSnapshotStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BatchedPolicyAckSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBatchedPolicyAckSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPolicyAckSnapshotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPolicyAckSnapshot item, double valueValue)
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

public sealed record CapBatchedPolicyAckSnapshotAdjuster(double Cap) : IBatchedPolicyAckSnapshotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPolicyAckSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched policy ack snapshot steps in order and keeps a trace of each.</summary>
public sealed class BatchedPolicyAckSnapshotEngine(List<IBatchedPolicyAckSnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPolicyAckSnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPolicyAckSnapshotEngine DefaultBatchedPolicyAckSnapshotEngine()
    {
        return new BatchedPolicyAckSnapshotEngine(
            new List<IBatchedPolicyAckSnapshotStep>
            {
                new TitleBatchedPolicyAckSnapshotAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.89, ["online"] = 0.75, ["east"] = 1.15 }),
                new TieredBatchedPolicyAckSnapshotAdjuster(
                    new List<double> { 21.0, 2841.0, 7225.0, 8866.0 },
                    new List<double> { 1.09, 1.07, 0.89, 0.88 }),
                new CapBatchedPolicyAckSnapshotAdjuster(36916.0),
            },
            new List<string>(),
            2.4,
            44560.1);
    }

    public double Compute(BatchedPolicyAckSnapshot item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPolicyAckSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
