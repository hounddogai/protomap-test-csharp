namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty audit calculation.</summary>
public interface IWarrantyAuditFactor
{
    string Describe();
    double Adjust(WarrantyAudit item, double valueValue);
}

public sealed record TieredWarrantyAuditAdjuster(List<double> Bounds, List<double> Rates) : IWarrantyAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantyAudit item, double valueValue)
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

public sealed record CapWarrantyAuditAdjuster(double Cap) : IWarrantyAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantyAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record UnitWarrantyAuditAdjuster(Dictionary<string, double> Factors) : IWarrantyAuditFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(WarrantyAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedWarrantyAuditAdjuster(Registry Flags, string Flag, double Percent) : IWarrantyAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantyAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWarrantyAuditAdjuster(double Step) : IWarrantyAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantyAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the warranty audit steps in order and keeps a trace of each.</summary>
public sealed class WarrantyAuditEngine(List<IWarrantyAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantyAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantyAuditEngine DefaultWarrantyAuditEngine(Registry flags)
    {
        return new WarrantyAuditEngine(
            new List<IWarrantyAuditFactor>
            {
                new TieredWarrantyAuditAdjuster(new List<double> { 2756.0, 6870.0 }, new List<double> { 1.07, 1.04 }),
                new CapWarrantyAuditAdjuster(42092.0),
                new UnitWarrantyAuditAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.09, ["east"] = 0.98, ["online"] = 1.24 }),
                new FlaggedWarrantyAuditAdjuster(flags, "warranty-audit-tiered", -4.6),
                new RoundingWarrantyAuditAdjuster(0.5),
            },
            new List<string>(),
            3.6,
            14160.8);
    }

    public double Compute(WarrantyAudit item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WarrantyAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
