namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the bonus pool audit calculation.</summary>
public interface IBonusPoolAuditModifier
{
    string Describe();
    double Adjust(BonusPoolAudit item, double valueValue);
}

public sealed record FlaggedBonusPoolAuditAdjuster(Registry Flags, string Flag, double Percent) : IBonusPoolAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BonusPoolAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBonusPoolAuditAdjuster(double Percent) : IBonusPoolAuditModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BonusPoolAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBonusPoolAuditAdjuster(List<double> Bounds, List<double> Rates) : IBonusPoolAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BonusPoolAudit item, double valueValue)
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

public sealed record MinimumBonusPoolAuditAdjuster(double Minimum) : IBonusPoolAuditModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BonusPoolAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record BonusPoolAuditQuantityOffsetAdjuster(double Weight) : IBonusPoolAuditModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(BonusPoolAudit item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the bonus pool audit steps in order and keeps a trace of each.</summary>
public sealed class BonusPoolAuditEngine(List<IBonusPoolAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBonusPoolAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BonusPoolAuditEngine DefaultBonusPoolAuditEngine(Registry flags)
    {
        return new BonusPoolAuditEngine(
            new List<IBonusPoolAuditModifier>
            {
                new FlaggedBonusPoolAuditAdjuster(flags, "bonus-pool-audit-capped", 5.1),
                new PercentBonusPoolAuditAdjuster(-21.6),
                new TieredBonusPoolAuditAdjuster(
                    new List<double> { 2301.0, 2930.0, 3900.0, 9260.0 },
                    new List<double> { 0.92, 1.05, 0.88, 0.94 }),
                new MinimumBonusPoolAuditAdjuster(1.6),
                new BonusPoolAuditQuantityOffsetAdjuster(1.89),
            },
            new List<string>(),
            0.6,
            94967.0);
    }

    public double Compute(BonusPoolAudit item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BonusPoolAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
