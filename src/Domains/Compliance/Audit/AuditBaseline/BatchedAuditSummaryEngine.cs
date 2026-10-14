namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched audit summary calculation.</summary>
public interface IBatchedAuditSummaryAdjuster
{
    string Describe();
    double Adjust(BatchedAuditSummary item, double valueValue);
}

public sealed record RoundingBatchedAuditSummaryAdjuster(double Step) : IBatchedAuditSummaryAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedAuditSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record BatchedAuditSummaryRevisionOffsetAdjuster(double Weight) : IBatchedAuditSummaryAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(BatchedAuditSummary item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record PercentBatchedAuditSummaryAdjuster(double Percent) : IBatchedAuditSummaryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedAuditSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched audit summary steps in order and keeps a trace of each.</summary>
public sealed class BatchedAuditSummaryEngine(List<IBatchedAuditSummaryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAuditSummaryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAuditSummaryEngine DefaultBatchedAuditSummaryEngine()
    {
        return new BatchedAuditSummaryEngine(
            new List<IBatchedAuditSummaryAdjuster>
            {
                new RoundingBatchedAuditSummaryAdjuster(0.1),
                new BatchedAuditSummaryRevisionOffsetAdjuster(0.31),
                new PercentBatchedAuditSummaryAdjuster(4.6),
            },
            new List<string>(),
            4.5,
            22534.6);
    }

    public double Evaluate(BatchedAuditSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAuditSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
