namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive warranty audit calculation.</summary>
public interface IAdaptiveWarrantyAuditModifier
{
    string Describe();
    double Adjust(AdaptiveWarrantyAudit item, double valueValue);
}

public sealed record FlaggedAdaptiveWarrantyAuditAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveWarrantyAuditModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveWarrantyAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAdaptiveWarrantyAuditAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveWarrantyAuditModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveWarrantyAudit item, double valueValue)
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

public sealed record CapAdaptiveWarrantyAuditAdjuster(double Cap) : IAdaptiveWarrantyAuditModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveWarrantyAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive warranty audit steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveWarrantyAuditEngine(List<IAdaptiveWarrantyAuditModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveWarrantyAuditModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveWarrantyAuditEngine DefaultAdaptiveWarrantyAuditEngine(Registry flags)
    {
        return new AdaptiveWarrantyAuditEngine(
            new List<IAdaptiveWarrantyAuditModifier>
            {
                new FlaggedAdaptiveWarrantyAuditAdjuster(flags, "adaptive-warranty-audit-adaptive", -4.3),
                new TieredAdaptiveWarrantyAuditAdjuster(
                    new List<double> { 8125.0, 9591.0 },
                    new List<double> { 0.98, 1.02 }),
                new CapAdaptiveWarrantyAuditAdjuster(14639.0),
            },
            new List<string>(),
            1.8,
            10591.0);
    }

    public double Price(AdaptiveWarrantyAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveWarrantyAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
