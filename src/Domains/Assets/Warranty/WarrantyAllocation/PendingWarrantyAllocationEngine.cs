namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending warranty allocation calculation.</summary>
public interface IPendingWarrantyAllocationStep
{
    string Describe();
    double Adjust(PendingWarrantyAllocation item, double valueValue);
}

public sealed record TieredPendingWarrantyAllocationAdjuster(List<double> Bounds, List<double> Rates) : IPendingWarrantyAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingWarrantyAllocation item, double valueValue)
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

public sealed record CapPendingWarrantyAllocationAdjuster(double Cap) : IPendingWarrantyAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingWarrantyAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPendingWarrantyAllocationAdjuster(double Step) : IPendingWarrantyAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingWarrantyAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionPendingWarrantyAllocationAdjuster(Dictionary<string, double> Factors) : IPendingWarrantyAllocationStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PendingWarrantyAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending warranty allocation steps in order and keeps a trace of each.</summary>
public sealed class PendingWarrantyAllocationEngine(List<IPendingWarrantyAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingWarrantyAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingWarrantyAllocationEngine DefaultPendingWarrantyAllocationEngine()
    {
        return new PendingWarrantyAllocationEngine(
            new List<IPendingWarrantyAllocationStep>
            {
                new TieredPendingWarrantyAllocationAdjuster(
                    new List<double> { 579.0, 2089.0, 2369.0, 7370.0 },
                    new List<double> { 1.04, 1.13, 0.92, 1.07 }),
                new CapPendingWarrantyAllocationAdjuster(13659.0),
                new RoundingPendingWarrantyAllocationAdjuster(0.05),
                new RegionPendingWarrantyAllocationAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.85, ["retail"] = 0.85 }),
            },
            new List<string>(),
            4.6,
            29397.2);
    }

    public double Evaluate(PendingWarrantyAllocation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingWarrantyAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
