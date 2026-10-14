namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched audit cycle calculation.</summary>
public interface IBatchedAuditCycleAdjuster
{
    string Describe();
    double Adjust(BatchedAuditCycle item, double valueValue);
}

public sealed record RoundingBatchedAuditCycleAdjuster(double Step) : IBatchedAuditCycleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedAuditCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryBatchedAuditCycleAdjuster(Dictionary<string, double> Factors) : IBatchedAuditCycleAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(BatchedAuditCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBatchedAuditCycleAdjuster(Registry Flags, string Flag, double Percent) : IBatchedAuditCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedAuditCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapBatchedAuditCycleAdjuster(double Cap) : IBatchedAuditCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedAuditCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the batched audit cycle steps in order and keeps a trace of each.</summary>
public sealed class BatchedAuditCycleEngine(List<IBatchedAuditCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedAuditCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedAuditCycleEngine DefaultBatchedAuditCycleEngine(Registry flags)
    {
        return new BatchedAuditCycleEngine(
            new List<IBatchedAuditCycleAdjuster>
            {
                new RoundingBatchedAuditCycleAdjuster(0.1),
                new CountryBatchedAuditCycleAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.82, ["west"] = 1.35, ["north"] = 1.12 }),
                new FlaggedBatchedAuditCycleAdjuster(flags, "batched-audit-cycle-primary", -0.8),
                new CapBatchedAuditCycleAdjuster(8543.0),
            },
            new List<string>(),
            2.2,
            57488.4);
    }

    public double Run(BatchedAuditCycle item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedAuditCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
