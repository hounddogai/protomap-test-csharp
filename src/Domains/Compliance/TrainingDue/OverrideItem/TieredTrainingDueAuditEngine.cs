namespace Acme.Workforce.Compliance.TrainingDue.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered training due audit calculation.</summary>
public interface ITieredTrainingDueAuditFactor
{
    string Describe();
    double Adjust(TieredTrainingDueAudit item, double valueValue);
}

public sealed record RoundingTieredTrainingDueAuditAdjuster(double Step) : ITieredTrainingDueAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredTrainingDueAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTrainingDueAuditPriorityOffsetAdjuster(double Weight) : ITieredTrainingDueAuditFactor
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(TieredTrainingDueAudit item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record CapTieredTrainingDueAuditAdjuster(double Cap) : ITieredTrainingDueAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredTrainingDueAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTieredTrainingDueAuditAdjuster(Registry Flags, string Flag, double Percent) : ITieredTrainingDueAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredTrainingDueAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTieredTrainingDueAuditAdjuster(List<double> Bounds, List<double> Rates) : ITieredTrainingDueAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredTrainingDueAudit item, double valueValue)
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

/// <summary>Runs the tiered training due audit steps in order and keeps a trace of each.</summary>
public sealed class TieredTrainingDueAuditEngine(List<ITieredTrainingDueAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredTrainingDueAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredTrainingDueAuditEngine DefaultTieredTrainingDueAuditEngine(Registry flags)
    {
        return new TieredTrainingDueAuditEngine(
            new List<ITieredTrainingDueAuditFactor>
            {
                new RoundingTieredTrainingDueAuditAdjuster(5.0),
                new TieredTrainingDueAuditPriorityOffsetAdjuster(0.46),
                new CapTieredTrainingDueAuditAdjuster(2622.0),
                new FlaggedTieredTrainingDueAuditAdjuster(flags, "tiered-training-due-audit-strict", 7.8),
                new TieredTieredTrainingDueAuditAdjuster(
                    new List<double> { 4132.0, 6744.0, 7329.0 },
                    new List<double> { 1.18, 0.91, 0.84 }),
            },
            new List<string>(),
            1.3,
            34028.2);
    }

    public double Evaluate(TieredTrainingDueAudit item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredTrainingDueAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
