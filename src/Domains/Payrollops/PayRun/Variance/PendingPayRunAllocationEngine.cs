namespace Acme.Workforce.Payrollops.PayRun.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pay run allocation calculation.</summary>
public interface IPendingPayRunAllocationModifier
{
    string Describe();
    double Adjust(PendingPayRunAllocation item, double valueValue);
}

public sealed record CountryPendingPayRunAllocationAdjuster(Dictionary<string, double> Factors) : IPendingPayRunAllocationModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(PendingPayRunAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingPayRunAllocationAdjuster(double Percent) : IPendingPayRunAllocationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingPayRunAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingPayRunAllocationAdjuster(double Step) : IPendingPayRunAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingPayRunAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPendingPayRunAllocationAdjuster(List<double> Bounds, List<double> Rates) : IPendingPayRunAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingPayRunAllocation item, double valueValue)
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

/// <summary>Runs the pending pay run allocation steps in order and keeps a trace of each.</summary>
public sealed class PendingPayRunAllocationEngine(List<IPendingPayRunAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPayRunAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPayRunAllocationEngine DefaultPendingPayRunAllocationEngine()
    {
        return new PendingPayRunAllocationEngine(
            new List<IPendingPayRunAllocationModifier>
            {
                new CountryPendingPayRunAllocationAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.35, ["west"] = 1.27, ["south"] = 1.04 }),
                new PercentPendingPayRunAllocationAdjuster(-21.5),
                new RoundingPendingPayRunAllocationAdjuster(1.0),
                new TieredPendingPayRunAllocationAdjuster(
                    new List<double> { 4216.0, 6193.0, 7491.0 },
                    new List<double> { 0.92, 1.07, 1.05 }),
            },
            new List<string>(),
            4.5,
            9829.7);
    }

    public double Evaluate(PendingPayRunAllocation item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingPayRunAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
