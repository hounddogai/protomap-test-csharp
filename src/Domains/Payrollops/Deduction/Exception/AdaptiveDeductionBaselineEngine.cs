namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive deduction baseline calculation.</summary>
public interface IAdaptiveDeductionBaselineModifier
{
    string Describe();
    double Adjust(AdaptiveDeductionBaseline item, double valueValue);
}

public sealed record AdaptiveDeductionBaselineRatingOffsetAdjuster(double Weight) : IAdaptiveDeductionBaselineModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(AdaptiveDeductionBaseline item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record RoundingAdaptiveDeductionBaselineAdjuster(double Step) : IAdaptiveDeductionBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveDeductionBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAdaptiveDeductionBaselineAdjuster(double Minimum) : IAdaptiveDeductionBaselineModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveDeductionBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveDeductionBaselineAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveDeductionBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveDeductionBaseline item, double valueValue)
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

/// <summary>Runs the adaptive deduction baseline steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveDeductionBaselineEngine(List<IAdaptiveDeductionBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveDeductionBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveDeductionBaselineEngine DefaultAdaptiveDeductionBaselineEngine()
    {
        return new AdaptiveDeductionBaselineEngine(
            new List<IAdaptiveDeductionBaselineModifier>
            {
                new AdaptiveDeductionBaselineRatingOffsetAdjuster(0.88),
                new RoundingAdaptiveDeductionBaselineAdjuster(0.1),
                new MinimumAdaptiveDeductionBaselineAdjuster(36.7),
                new TieredAdaptiveDeductionBaselineAdjuster(
                    new List<double> { 422.0, 1961.0, 9269.0, 9376.0 },
                    new List<double> { 1.05, 1.06, 0.96, 1.14 }),
            },
            new List<string>(),
            4.8,
            18318.6);
    }

    public double Evaluate(AdaptiveDeductionBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveDeductionBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
