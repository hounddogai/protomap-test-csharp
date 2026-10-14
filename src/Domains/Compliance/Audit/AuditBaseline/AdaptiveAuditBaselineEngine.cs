namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive audit baseline calculation.</summary>
public interface IAdaptiveAuditBaselineModifier
{
    string Describe();
    double Adjust(AdaptiveAuditBaseline item, double valueValue);
}

public sealed record FlaggedAdaptiveAuditBaselineAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveAuditBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveAuditBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveAuditBaselineAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveAuditBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveAuditBaseline item, double valueValue)
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

public sealed record AdaptiveAuditBaselineMaximumOffsetAdjuster(double Weight) : IAdaptiveAuditBaselineModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(AdaptiveAuditBaseline item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the adaptive audit baseline steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveAuditBaselineEngine(List<IAdaptiveAuditBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveAuditBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveAuditBaselineEngine DefaultAdaptiveAuditBaselineEngine(Registry flags)
    {
        return new AdaptiveAuditBaselineEngine(
            new List<IAdaptiveAuditBaselineModifier>
            {
                new FlaggedAdaptiveAuditBaselineAdjuster(flags, "adaptive-audit-baseline-default", 6.5),
                new TieredAdaptiveAuditBaselineAdjuster(
                    new List<double> { 2020.0, 3214.0, 4302.0 },
                    new List<double> { 1.09, 1.17, 1.05 }),
                new AdaptiveAuditBaselineMaximumOffsetAdjuster(1.28),
            },
            new List<string>(),
            1.6,
            18503.3);
    }

    public double Price(AdaptiveAuditBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveAuditBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
