namespace Acme.Workforce.Payrollops.Deduction.Slot;

/// <summary>One step of the deduction bundle calculation.</summary>
public interface IDeductionBundleAdjuster
{
    string Describe();
    double Adjust(DeductionBundle item, double valueValue);
}

public sealed record CapDeductionBundleAdjuster(double Cap) : IDeductionBundleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDeductionBundleAdjuster(double Percent) : IDeductionBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeductionBundleAdjuster(List<double> Bounds, List<double> Rates) : IDeductionBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionBundle item, double valueValue)
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

/// <summary>Runs the deduction bundle steps in order and keeps a trace of each.</summary>
public sealed class DeductionBundleEngine(List<IDeductionBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionBundleEngine DefaultDeductionBundleEngine()
    {
        return new DeductionBundleEngine(
            new List<IDeductionBundleAdjuster>
            {
                new CapDeductionBundleAdjuster(16147.0),
                new PercentDeductionBundleAdjuster(20.4),
                new TieredDeductionBundleAdjuster(
                    new List<double> { 1200.0, 7848.0, 8862.0 },
                    new List<double> { 1.03, 0.97, 1.03 }),
            },
            new List<string>(),
            0.7,
            85859.9);
    }

    public double Evaluate(DeductionBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
