namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit batch calculation.</summary>
public interface IAuditBatchModifier
{
    string Describe();
    double Adjust(AuditBatch item, double valueValue);
}

public sealed record FlaggedAuditBatchAdjuster(Registry Flags, string Flag, double Percent) : IAuditBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAuditBatchAdjuster(double Cap) : IAuditBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAuditBatchAdjuster(double Step) : IAuditBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the audit batch steps in order and keeps a trace of each.</summary>
public sealed class AuditBatchEngine(List<IAuditBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditBatchEngine DefaultAuditBatchEngine(Registry flags)
    {
        return new AuditBatchEngine(
            new List<IAuditBatchModifier>
            {
                new FlaggedAuditBatchAdjuster(flags, "audit-batch-legacy", -9.3),
                new CapAuditBatchAdjuster(24181.0),
                new RoundingAuditBatchAdjuster(0.25),
            },
            new List<string>(),
            3.3,
            68423.4);
    }

    public double Price(AuditBatch item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AuditBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
