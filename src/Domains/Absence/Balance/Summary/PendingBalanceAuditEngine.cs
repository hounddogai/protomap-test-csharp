namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending balance audit calculation.</summary>
public interface IPendingBalanceAuditModifier
{
    string Describe();
    double Adjust(PendingBalanceAudit item, double valueValue);
}

public sealed record LabelPendingBalanceAuditAdjuster(Dictionary<string, double> Factors) : IPendingBalanceAuditModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PendingBalanceAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPendingBalanceAuditAdjuster(List<double> Bounds, List<double> Rates) : IPendingBalanceAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingBalanceAudit item, double valueValue)
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

public sealed record CapPendingBalanceAuditAdjuster(double Cap) : IPendingBalanceAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingBalanceAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPendingBalanceAuditAdjuster(Registry Flags, string Flag, double Percent) : IPendingBalanceAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingBalanceAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingBalanceAuditAdjuster(double Step) : IPendingBalanceAuditModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingBalanceAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PendingBalanceAuditTenureMonthsOffsetAdjuster(double Weight) : IPendingBalanceAuditModifier
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(PendingBalanceAudit item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the pending balance audit steps in order and keeps a trace of each.</summary>
public sealed class PendingBalanceAuditEngine(List<IPendingBalanceAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingBalanceAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingBalanceAuditEngine DefaultPendingBalanceAuditEngine(Registry flags)
    {
        return new PendingBalanceAuditEngine(
            new List<IPendingBalanceAuditModifier>
            {
                new LabelPendingBalanceAuditAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.06, ["online"] = 0.75, ["central"] = 1.24 }),
                new TieredPendingBalanceAuditAdjuster(
                    new List<double> { 3596.0, 5229.0, 6175.0, 6605.0 },
                    new List<double> { 1.01, 0.98, 1.1, 1.0 }),
                new CapPendingBalanceAuditAdjuster(13099.0),
                new FlaggedPendingBalanceAuditAdjuster(flags, "pending-balance-audit-capped", 0.7),
                new RoundingPendingBalanceAuditAdjuster(0.5),
                new PendingBalanceAuditTenureMonthsOffsetAdjuster(0.93),
            },
            new List<string>(),
            2.2,
            38167.5);
    }

    public double Evaluate(PendingBalanceAudit item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingBalanceAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
