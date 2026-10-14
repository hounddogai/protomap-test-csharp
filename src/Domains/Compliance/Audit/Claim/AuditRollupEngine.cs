namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit rollup calculation.</summary>
public interface IAuditRollupModifier
{
    string Describe();
    double Adjust(AuditRollup item, double valueValue);
}

public sealed record AuditRollupUpdatedAtOffsetAdjuster(double Weight) : IAuditRollupModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AuditRollup item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record FlaggedAuditRollupAdjuster(Registry Flags, string Flag, double Percent) : IAuditRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAuditRollupAdjuster(double Minimum) : IAuditRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the audit rollup steps in order and keeps a trace of each.</summary>
public sealed class AuditRollupEngine(List<IAuditRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditRollupEngine DefaultAuditRollupEngine(Registry flags)
    {
        return new AuditRollupEngine(
            new List<IAuditRollupModifier>
            {
                new AuditRollupUpdatedAtOffsetAdjuster(0.41),
                new FlaggedAuditRollupAdjuster(flags, "audit-rollup-batched", -4.1),
                new MinimumAuditRollupAdjuster(7.4),
            },
            new List<string>(),
            3.1,
            87233.7);
    }

    public double Run(AuditRollup item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AuditRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
