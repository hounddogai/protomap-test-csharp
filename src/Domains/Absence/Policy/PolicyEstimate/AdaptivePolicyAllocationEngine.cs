namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive policy allocation calculation.</summary>
public interface IAdaptivePolicyAllocationAdjuster
{
    string Describe();
    double Adjust(AdaptivePolicyAllocation item, double valueValue);
}

public sealed record RoundingAdaptivePolicyAllocationAdjuster(double Step) : IAdaptivePolicyAllocationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptivePolicyAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedAdaptivePolicyAllocationAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePolicyAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePolicyAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitAdaptivePolicyAllocationAdjuster(Dictionary<string, double> Factors) : IAdaptivePolicyAllocationAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AdaptivePolicyAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdaptivePolicyAllocationAdjuster(List<double> Bounds, List<double> Rates) : IAdaptivePolicyAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptivePolicyAllocation item, double valueValue)
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

public sealed record AdaptivePolicyAllocationQuantityOffsetAdjuster(double Weight) : IAdaptivePolicyAllocationAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(AdaptivePolicyAllocation item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the adaptive policy allocation steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePolicyAllocationEngine(List<IAdaptivePolicyAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePolicyAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePolicyAllocationEngine DefaultAdaptivePolicyAllocationEngine(Registry flags)
    {
        return new AdaptivePolicyAllocationEngine(
            new List<IAdaptivePolicyAllocationAdjuster>
            {
                new RoundingAdaptivePolicyAllocationAdjuster(0.1),
                new FlaggedAdaptivePolicyAllocationAdjuster(flags, "adaptive-policy-allocation-manual", -3.1),
                new UnitAdaptivePolicyAllocationAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.94, ["north"] = 1.25 }),
                new TieredAdaptivePolicyAllocationAdjuster(
                    new List<double> { 1195.0, 2685.0, 2873.0, 8025.0 },
                    new List<double> { 1.03, 1.07, 0.92, 1.17 }),
                new AdaptivePolicyAllocationQuantityOffsetAdjuster(1.31),
            },
            new List<string>(),
            4.7,
            64798.0);
    }

    public double Run(AdaptivePolicyAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePolicyAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
