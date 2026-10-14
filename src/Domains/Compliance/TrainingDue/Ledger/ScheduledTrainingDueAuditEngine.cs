namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled training due audit calculation.</summary>
public interface IScheduledTrainingDueAuditAdjuster
{
    string Describe();
    double Adjust(ScheduledTrainingDueAudit item, double valueValue);
}

public sealed record TieredScheduledTrainingDueAuditAdjuster(List<double> Bounds, List<double> Rates) : IScheduledTrainingDueAuditAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledTrainingDueAudit item, double valueValue)
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

public sealed record FlaggedScheduledTrainingDueAuditAdjuster(Registry Flags, string Flag, double Percent) : IScheduledTrainingDueAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledTrainingDueAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameScheduledTrainingDueAuditAdjuster(Dictionary<string, double> Factors) : IScheduledTrainingDueAuditAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledTrainingDueAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledTrainingDueAuditAdjuster(double Cap) : IScheduledTrainingDueAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledTrainingDueAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScheduledTrainingDueAuditAdjuster(double Step) : IScheduledTrainingDueAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledTrainingDueAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled training due audit steps in order and keeps a trace of each.</summary>
public sealed class ScheduledTrainingDueAuditEngine(List<IScheduledTrainingDueAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledTrainingDueAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledTrainingDueAuditEngine DefaultScheduledTrainingDueAuditEngine(Registry flags)
    {
        return new ScheduledTrainingDueAuditEngine(
            new List<IScheduledTrainingDueAuditAdjuster>
            {
                new TieredScheduledTrainingDueAuditAdjuster(
                    new List<double> { 103.0, 5699.0 },
                    new List<double> { 0.88, 1.06 }),
                new FlaggedScheduledTrainingDueAuditAdjuster(flags, "scheduled-training-due-audit-capped", 3.0),
                new NameScheduledTrainingDueAuditAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.82, ["south"] = 0.8, ["retail"] = 1.05 }),
                new CapScheduledTrainingDueAuditAdjuster(15628.0),
                new RoundingScheduledTrainingDueAuditAdjuster(0.25),
            },
            new List<string>(),
            0.0,
            50221.6);
    }

    public double Run(ScheduledTrainingDueAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledTrainingDueAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
