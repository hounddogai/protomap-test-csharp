namespace Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>One step of the batched pool audit calculation.</summary>
public interface IBatchedPoolAuditModifier
{
    string Describe();
    double Adjust(BatchedPoolAudit item, double valueValue);
}

public sealed record CapBatchedPoolAuditAdjuster(double Cap) : IBatchedPoolAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPoolAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumBatchedPoolAuditAdjuster(double Minimum) : IBatchedPoolAuditModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedPoolAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameBatchedPoolAuditAdjuster(Dictionary<string, double> Factors) : IBatchedPoolAuditModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BatchedPoolAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBatchedPoolAuditAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPoolAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPoolAudit item, double valueValue)
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

/// <summary>Runs the batched pool audit steps in order and keeps a trace of each.</summary>
public sealed class BatchedPoolAuditEngine(List<IBatchedPoolAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPoolAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPoolAuditEngine DefaultBatchedPoolAuditEngine()
    {
        return new BatchedPoolAuditEngine(
            new List<IBatchedPoolAuditModifier>
            {
                new CapBatchedPoolAuditAdjuster(5628.0),
                new MinimumBatchedPoolAuditAdjuster(13.5),
                new NameBatchedPoolAuditAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.89, ["north"] = 1.27 }),
                new TieredBatchedPoolAuditAdjuster(
                    new List<double> { 1775.0, 7018.0, 7749.0 },
                    new List<double> { 0.82, 1.17, 1.14 }),
            },
            new List<string>(),
            0.5,
            25381.4);
    }

    public double Compute(BatchedPoolAudit item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPoolAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
