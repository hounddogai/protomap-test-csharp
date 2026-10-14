namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted laptop audit calculation.</summary>
public interface IWeightedLaptopAuditAdjuster
{
    string Describe();
    double Adjust(WeightedLaptopAudit item, double valueValue);
}

public sealed record FlaggedWeightedLaptopAuditAdjuster(Registry Flags, string Flag, double Percent) : IWeightedLaptopAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedLaptopAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedLaptopAuditAdjuster(double Step) : IWeightedLaptopAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedLaptopAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapWeightedLaptopAuditAdjuster(double Cap) : IWeightedLaptopAuditAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedLaptopAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the weighted laptop audit steps in order and keeps a trace of each.</summary>
public sealed class WeightedLaptopAuditEngine(List<IWeightedLaptopAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedLaptopAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedLaptopAuditEngine DefaultWeightedLaptopAuditEngine(Registry flags)
    {
        return new WeightedLaptopAuditEngine(
            new List<IWeightedLaptopAuditAdjuster>
            {
                new FlaggedWeightedLaptopAuditAdjuster(flags, "weighted-laptop-audit-pending", 0.2),
                new RoundingWeightedLaptopAuditAdjuster(0.05),
                new CapWeightedLaptopAuditAdjuster(14959.0),
            },
            new List<string>(),
            0.0,
            4391.8);
    }

    public double Run(WeightedLaptopAudit item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedLaptopAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
