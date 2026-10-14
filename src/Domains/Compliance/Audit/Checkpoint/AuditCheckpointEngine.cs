namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit checkpoint calculation.</summary>
public interface IAuditCheckpointStep
{
    string Describe();
    double Adjust(AuditCheckpoint item, double valueValue);
}

public sealed record AuditCheckpointLeaveDaysOffsetAdjuster(double Weight) : IAuditCheckpointStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(AuditCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record PercentAuditCheckpointAdjuster(double Percent) : IAuditCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAuditCheckpointAdjuster(double Cap) : IAuditCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAuditCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IAuditCheckpointStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditCheckpoint item, double valueValue)
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

public sealed record FlaggedAuditCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IAuditCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the audit checkpoint steps in order and keeps a trace of each.</summary>
public sealed class AuditCheckpointEngine(List<IAuditCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditCheckpointEngine DefaultAuditCheckpointEngine(Registry flags)
    {
        return new AuditCheckpointEngine(
            new List<IAuditCheckpointStep>
            {
                new AuditCheckpointLeaveDaysOffsetAdjuster(0.22),
                new PercentAuditCheckpointAdjuster(-19.7),
                new CapAuditCheckpointAdjuster(44044.0),
                new TieredAuditCheckpointAdjuster(
                    new List<double> { 2313.0, 2752.0, 4532.0 },
                    new List<double> { 0.89, 1.11, 1.17 }),
                new FlaggedAuditCheckpointAdjuster(flags, "audit-checkpoint-fallback", -8.0),
            },
            new List<string>(),
            0.7,
            65651.8);
    }

    public double Compute(AuditCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<AuditCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
