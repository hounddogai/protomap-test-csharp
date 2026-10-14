namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit threshold calculation.</summary>
public interface IAuditThresholdModifier
{
    string Describe();
    double Adjust(AuditThreshold item, double valueValue);
}

public sealed record SourceAuditThresholdAdjuster(Dictionary<string, double> Factors) : IAuditThresholdModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AuditThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAuditThresholdAdjuster(List<double> Bounds, List<double> Rates) : IAuditThresholdModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditThreshold item, double valueValue)
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

public sealed record CapAuditThresholdAdjuster(double Cap) : IAuditThresholdModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAuditThresholdAdjuster(Registry Flags, string Flag, double Percent) : IAuditThresholdModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAuditThresholdAdjuster(double Percent) : IAuditThresholdModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AuditThresholdQuantityOffsetAdjuster(double Weight) : IAuditThresholdModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(AuditThreshold item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the audit threshold steps in order and keeps a trace of each.</summary>
public sealed class AuditThresholdEngine(List<IAuditThresholdModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditThresholdModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditThresholdEngine DefaultAuditThresholdEngine(Registry flags)
    {
        return new AuditThresholdEngine(
            new List<IAuditThresholdModifier>
            {
                new SourceAuditThresholdAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.79, ["central"] = 0.94, ["east"] = 1.33 }),
                new TieredAuditThresholdAdjuster(
                    new List<double> { 4871.0, 7215.0, 7593.0 },
                    new List<double> { 1.07, 1.11, 0.87 }),
                new CapAuditThresholdAdjuster(46860.0),
                new FlaggedAuditThresholdAdjuster(flags, "audit-threshold-legacy", 6.1),
                new PercentAuditThresholdAdjuster(-17.1),
                new AuditThresholdQuantityOffsetAdjuster(0.62),
            },
            new List<string>(),
            1.0,
            11061.9);
    }

    public double Compute(AuditThreshold item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AuditThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
