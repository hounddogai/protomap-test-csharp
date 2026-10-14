namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary audit mapping calculation.</summary>
public interface IPrimaryAuditMappingStep
{
    string Describe();
    double Adjust(PrimaryAuditMapping item, double valueValue);
}

public sealed record TieredPrimaryAuditMappingAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryAuditMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryAuditMapping item, double valueValue)
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

public sealed record FlaggedPrimaryAuditMappingAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryAuditMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryAuditMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocalePrimaryAuditMappingAdjuster(Dictionary<string, double> Factors) : IPrimaryAuditMappingStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PrimaryAuditMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPrimaryAuditMappingAdjuster(double Percent) : IPrimaryAuditMappingStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAuditMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryAuditMappingDurationMinutesOffsetAdjuster(double Weight) : IPrimaryAuditMappingStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(PrimaryAuditMapping item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

/// <summary>Runs the primary audit mapping steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAuditMappingEngine(List<IPrimaryAuditMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAuditMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAuditMappingEngine DefaultPrimaryAuditMappingEngine(Registry flags)
    {
        return new PrimaryAuditMappingEngine(
            new List<IPrimaryAuditMappingStep>
            {
                new TieredPrimaryAuditMappingAdjuster(
                    new List<double> { 113.0, 2366.0, 4249.0, 4549.0 },
                    new List<double> { 0.99, 0.95, 1.08, 0.94 }),
                new FlaggedPrimaryAuditMappingAdjuster(flags, "primary-audit-mapping-fallback", -6.1),
                new LocalePrimaryAuditMappingAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.71, ["west"] = 1.06 }),
                new PercentPrimaryAuditMappingAdjuster(24.5),
                new PrimaryAuditMappingDurationMinutesOffsetAdjuster(0.14),
            },
            new List<string>(),
            1.6,
            3807.6);
    }

    public double Run(PrimaryAuditMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAuditMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
