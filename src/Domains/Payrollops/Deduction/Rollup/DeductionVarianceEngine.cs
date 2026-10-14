namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the deduction variance calculation.</summary>
public interface IDeductionVarianceFactor
{
    string Describe();
    double Adjust(DeductionVariance item, double valueValue);
}

public sealed record MinimumDeductionVarianceAdjuster(double Minimum) : IDeductionVarianceFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDeductionVarianceAdjuster(double Percent) : IDeductionVarianceFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredDeductionVarianceAdjuster(List<double> Bounds, List<double> Rates) : IDeductionVarianceFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionVariance item, double valueValue)
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

public sealed record FlaggedDeductionVarianceAdjuster(Registry Flags, string Flag, double Percent) : IDeductionVarianceFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the deduction variance steps in order and keeps a trace of each.</summary>
public sealed class DeductionVarianceEngine(List<IDeductionVarianceFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionVarianceFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionVarianceEngine DefaultDeductionVarianceEngine(Registry flags)
    {
        return new DeductionVarianceEngine(
            new List<IDeductionVarianceFactor>
            {
                new MinimumDeductionVarianceAdjuster(13.5),
                new PercentDeductionVarianceAdjuster(-8.7),
                new TieredDeductionVarianceAdjuster(
                    new List<double> { 3313.0, 7892.0 },
                    new List<double> { 0.84, 0.81 }),
                new FlaggedDeductionVarianceAdjuster(flags, "deduction-variance-manual", 4.5),
            },
            new List<string>(),
            4.1,
            2055.9);
    }

    public double Evaluate(DeductionVariance item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
