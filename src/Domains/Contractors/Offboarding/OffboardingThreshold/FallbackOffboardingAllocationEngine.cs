namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>One step of the fallback offboarding allocation calculation.</summary>
public interface IFallbackOffboardingAllocationAdjuster
{
    string Describe();
    double Adjust(FallbackOffboardingAllocation item, double valueValue);
}

public sealed record SourceFallbackOffboardingAllocationAdjuster(Dictionary<string, double> Factors) : IFallbackOffboardingAllocationAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(FallbackOffboardingAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapFallbackOffboardingAllocationAdjuster(double Cap) : IFallbackOffboardingAllocationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackOffboardingAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackOffboardingAllocationAdjuster(List<double> Bounds, List<double> Rates) : IFallbackOffboardingAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackOffboardingAllocation item, double valueValue)
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

/// <summary>Runs the fallback offboarding allocation steps in order and keeps a trace of each.</summary>
public sealed class FallbackOffboardingAllocationEngine(List<IFallbackOffboardingAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackOffboardingAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackOffboardingAllocationEngine DefaultFallbackOffboardingAllocationEngine()
    {
        return new FallbackOffboardingAllocationEngine(
            new List<IFallbackOffboardingAllocationAdjuster>
            {
                new SourceFallbackOffboardingAllocationAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.0, ["west"] = 1.1 }),
                new CapFallbackOffboardingAllocationAdjuster(31456.0),
                new TieredFallbackOffboardingAllocationAdjuster(
                    new List<double> { 3815.0, 7417.0, 8718.0, 8905.0 },
                    new List<double> { 0.93, 0.8, 0.95, 1.05 }),
            },
            new List<string>(),
            2.9,
            85987.3);
    }

    public double Compute(FallbackOffboardingAllocation item)
    {
        var current = item.EffectiveValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FallbackOffboardingAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
