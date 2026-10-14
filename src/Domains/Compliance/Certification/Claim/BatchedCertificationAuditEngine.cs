namespace Acme.Workforce.Compliance.Certification.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched certification audit calculation.</summary>
public interface IBatchedCertificationAuditFactor
{
    string Describe();
    double Adjust(BatchedCertificationAudit item, double valueValue);
}

public sealed record MinimumBatchedCertificationAuditAdjuster(double Minimum) : IBatchedCertificationAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedCertificationAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapBatchedCertificationAuditAdjuster(double Cap) : IBatchedCertificationAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedCertificationAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SegmentBatchedCertificationAuditAdjuster(Dictionary<string, double> Factors) : IBatchedCertificationAuditFactor
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BatchedCertificationAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBatchedCertificationAuditAdjuster(List<double> Bounds, List<double> Rates) : IBatchedCertificationAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedCertificationAudit item, double valueValue)
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

/// <summary>Runs the batched certification audit steps in order and keeps a trace of each.</summary>
public sealed class BatchedCertificationAuditEngine(List<IBatchedCertificationAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedCertificationAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedCertificationAuditEngine DefaultBatchedCertificationAuditEngine()
    {
        return new BatchedCertificationAuditEngine(
            new List<IBatchedCertificationAuditFactor>
            {
                new MinimumBatchedCertificationAuditAdjuster(46.3),
                new CapBatchedCertificationAuditAdjuster(20416.0),
                new SegmentBatchedCertificationAuditAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.86, ["central"] = 0.9 }),
                new TieredBatchedCertificationAuditAdjuster(
                    new List<double> { 234.0, 856.0, 9257.0 },
                    new List<double> { 0.86, 0.99, 1.03 }),
            },
            new List<string>(),
            1.3,
            30536.1);
    }

    public double Compute(BatchedCertificationAudit item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedCertificationAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
