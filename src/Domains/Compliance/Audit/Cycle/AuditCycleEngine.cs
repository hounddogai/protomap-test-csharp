namespace Acme.Workforce.Compliance.Audit.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit cycle calculation.</summary>
public interface IAuditCycleFactor
{
    string Describe();
    double Adjust(AuditCycle item, double valueValue);
}

public sealed record FlaggedAuditCycleAdjuster(Registry Flags, string Flag, double Percent) : IAuditCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAuditCycleAdjuster(double Minimum) : IAuditCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAuditCycleAdjuster(double Cap) : IAuditCycleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAuditCycleAdjuster(List<double> Bounds, List<double> Rates) : IAuditCycleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditCycle item, double valueValue)
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

public sealed record UnitAuditCycleAdjuster(Dictionary<string, double> Factors) : IAuditCycleFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AuditCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAuditCycleAdjuster(double Step) : IAuditCycleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the audit cycle steps in order and keeps a trace of each.</summary>
public sealed class AuditCycleEngine(List<IAuditCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditCycleEngine DefaultAuditCycleEngine(Registry flags)
    {
        return new AuditCycleEngine(
            new List<IAuditCycleFactor>
            {
                new FlaggedAuditCycleAdjuster(flags, "audit-cycle-default", 6.9),
                new MinimumAuditCycleAdjuster(19.7),
                new CapAuditCycleAdjuster(4163.0),
                new TieredAuditCycleAdjuster(
                    new List<double> { 964.0, 1989.0, 6373.0, 8366.0 },
                    new List<double> { 0.89, 1.14, 0.98, 0.84 }),
                new UnitAuditCycleAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["retail"] = 1.38, ["online"] = 0.96 }),
                new RoundingAuditCycleAdjuster(0.25),
            },
            new List<string>(),
            4.1,
            26400.8);
    }

    public double Run(AuditCycle item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AuditCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
