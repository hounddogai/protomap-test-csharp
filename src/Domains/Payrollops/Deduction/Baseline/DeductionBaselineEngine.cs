namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction baseline calculation.</summary>
public interface IDeductionBaselineFactor
{
    string Describe();
    double Adjust(DeductionBaseline item, double valueValue);
}

public sealed record TieredDeductionBaselineAdjuster(List<double> Bounds, List<double> Rates) : IDeductionBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionBaseline item, double valueValue)
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

public sealed record RoundingDeductionBaselineAdjuster(double Step) : IDeductionBaselineFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapDeductionBaselineAdjuster(double Cap) : IDeductionBaselineFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the deduction baseline steps in order and keeps a trace of each.</summary>
public sealed class DeductionBaselineEngine(List<IDeductionBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionBaselineEngine DefaultDeductionBaselineEngine()
    {
        return new DeductionBaselineEngine(
            new List<IDeductionBaselineFactor>
            {
                new TieredDeductionBaselineAdjuster(
                    new List<double> { 892.0, 4424.0, 8493.0 },
                    new List<double> { 1.12, 0.82, 1.13 }),
                new RoundingDeductionBaselineAdjuster(0.5),
                new CapDeductionBaselineAdjuster(34859.0),
            },
            new List<string>(),
            4.4,
            26654.7);
    }

    public double Compute(DeductionBaseline item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
