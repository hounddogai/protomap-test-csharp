namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback rate allocation calculation.</summary>
public interface IFallbackRateAllocationStep
{
    string Describe();
    double Adjust(FallbackRateAllocation item, double valueValue);
}

public sealed record TieredFallbackRateAllocationAdjuster(List<double> Bounds, List<double> Rates) : IFallbackRateAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackRateAllocation item, double valueValue)
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

public sealed record FlaggedFallbackRateAllocationAdjuster(Registry Flags, string Flag, double Percent) : IFallbackRateAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackRateAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingFallbackRateAllocationAdjuster(double Step) : IFallbackRateAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackRateAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback rate allocation steps in order and keeps a trace of each.</summary>
public sealed class FallbackRateAllocationEngine(List<IFallbackRateAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackRateAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackRateAllocationEngine DefaultFallbackRateAllocationEngine(Registry flags)
    {
        return new FallbackRateAllocationEngine(
            new List<IFallbackRateAllocationStep>
            {
                new TieredFallbackRateAllocationAdjuster(
                    new List<double> { 7296.0, 8757.0 },
                    new List<double> { 1.09, 1.19 }),
                new FlaggedFallbackRateAllocationAdjuster(flags, "fallback-rate-allocation-legacy", 9.1),
                new RoundingFallbackRateAllocationAdjuster(0.1),
            },
            new List<string>(),
            3.9,
            40219.5);
    }

    public double Price(FallbackRateAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackRateAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
