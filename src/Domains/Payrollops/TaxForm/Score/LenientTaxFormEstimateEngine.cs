namespace Acme.Workforce.Payrollops.TaxForm.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient tax form estimate calculation.</summary>
public interface ILenientTaxFormEstimateFactor
{
    string Describe();
    double Adjust(LenientTaxFormEstimate item, double valueValue);
}

public sealed record DepartmentLenientTaxFormEstimateAdjuster(Dictionary<string, double> Factors) : ILenientTaxFormEstimateFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LenientTaxFormEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientTaxFormEstimateAdjuster(Registry Flags, string Flag, double Percent) : ILenientTaxFormEstimateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientTaxFormEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientTaxFormEstimateAdjuster(double Step) : ILenientTaxFormEstimateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientTaxFormEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLenientTaxFormEstimateAdjuster(double Minimum) : ILenientTaxFormEstimateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientTaxFormEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LenientTaxFormEstimateVersionOffsetAdjuster(double Weight) : ILenientTaxFormEstimateFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(LenientTaxFormEstimate item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record TieredLenientTaxFormEstimateAdjuster(List<double> Bounds, List<double> Rates) : ILenientTaxFormEstimateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientTaxFormEstimate item, double valueValue)
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

/// <summary>Runs the lenient tax form estimate steps in order and keeps a trace of each.</summary>
public sealed class LenientTaxFormEstimateEngine(List<ILenientTaxFormEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientTaxFormEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientTaxFormEstimateEngine DefaultLenientTaxFormEstimateEngine(Registry flags)
    {
        return new LenientTaxFormEstimateEngine(
            new List<ILenientTaxFormEstimateFactor>
            {
                new DepartmentLenientTaxFormEstimateAdjuster(new Dictionary<string, double> { ["retail"] = 1.11 }),
                new FlaggedLenientTaxFormEstimateAdjuster(flags, "lenient-tax-form-estimate-scheduled", -1.3),
                new RoundingLenientTaxFormEstimateAdjuster(1.0),
                new MinimumLenientTaxFormEstimateAdjuster(30.8),
                new LenientTaxFormEstimateVersionOffsetAdjuster(0.77),
                new TieredLenientTaxFormEstimateAdjuster(
                    new List<double> { 147.0, 1967.0, 3975.0, 7875.0 },
                    new List<double> { 0.97, 1.0, 0.99, 0.98 }),
            },
            new List<string>(),
            2.3,
            20598.9);
    }

    public double Run(LenientTaxFormEstimate item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientTaxFormEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
