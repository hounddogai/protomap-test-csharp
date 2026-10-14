namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the certification audit calculation.</summary>
public interface ICertificationAuditModifier
{
    string Describe();
    double Adjust(CertificationAudit item, double valueValue);
}

public sealed record FlaggedCertificationAuditAdjuster(Registry Flags, string Flag, double Percent) : ICertificationAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CertificationAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CertificationAuditStartsAtOffsetAdjuster(double Weight) : ICertificationAuditModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(CertificationAudit item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record RoundingCertificationAuditAdjuster(double Step) : ICertificationAuditModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CertificationAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the certification audit steps in order and keeps a trace of each.</summary>
public sealed class CertificationAuditEngine(List<ICertificationAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICertificationAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CertificationAuditEngine DefaultCertificationAuditEngine(Registry flags)
    {
        return new CertificationAuditEngine(
            new List<ICertificationAuditModifier>
            {
                new FlaggedCertificationAuditAdjuster(flags, "certification-audit-global", 7.1),
                new CertificationAuditStartsAtOffsetAdjuster(0.76),
                new RoundingCertificationAuditAdjuster(0.05),
            },
            new List<string>(),
            2.6,
            22359.8);
    }

    public double Compute(CertificationAudit item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CertificationAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
