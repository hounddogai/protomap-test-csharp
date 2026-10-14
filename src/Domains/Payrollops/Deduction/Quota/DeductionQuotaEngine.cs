namespace Acme.Workforce.Payrollops.Deduction.Quota;

/// <summary>One step of the deduction quota calculation.</summary>
public interface IDeductionQuotaModifier
{
    string Describe();
    double Adjust(DeductionQuota item, double valueValue);
}

public sealed record CountryDeductionQuotaAdjuster(Dictionary<string, double> Factors) : IDeductionQuotaModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DeductionQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDeductionQuotaAdjuster(List<double> Bounds, List<double> Rates) : IDeductionQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionQuota item, double valueValue)
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

public sealed record PercentDeductionQuotaAdjuster(double Percent) : IDeductionQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the deduction quota steps in order and keeps a trace of each.</summary>
public sealed class DeductionQuotaEngine(List<IDeductionQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionQuotaEngine DefaultDeductionQuotaEngine()
    {
        return new DeductionQuotaEngine(
            new List<IDeductionQuotaModifier>
            {
                new CountryDeductionQuotaAdjuster(new Dictionary<string, double> { ["north"] = 0.95 }),
                new TieredDeductionQuotaAdjuster(
                    new List<double> { 1125.0, 3977.0 },
                    new List<double> { 1.12, 1.05 }),
                new PercentDeductionQuotaAdjuster(-9.6),
            },
            new List<string>(),
            3.4,
            45409.4);
    }

    public double Run(DeductionQuota item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
