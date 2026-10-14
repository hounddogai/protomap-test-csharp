namespace Acme.Workforce.Absence.Policy.Baseline;

/// <summary>One step of the lenient policy adjustment calculation.</summary>
public interface ILenientPolicyAdjustmentModifier
{
    string Describe();
    double Adjust(LenientPolicyAdjustment item, double valueValue);
}

public sealed record PercentLenientPolicyAdjustmentAdjuster(double Percent) : ILenientPolicyAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientPolicyAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientPolicyAdjustmentAdjuster(double Minimum) : ILenientPolicyAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientPolicyAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLenientPolicyAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : ILenientPolicyAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientPolicyAdjustment item, double valueValue)
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

/// <summary>Runs the lenient policy adjustment steps in order and keeps a trace of each.</summary>
public sealed class LenientPolicyAdjustmentEngine(List<ILenientPolicyAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientPolicyAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientPolicyAdjustmentEngine DefaultLenientPolicyAdjustmentEngine()
    {
        return new LenientPolicyAdjustmentEngine(
            new List<ILenientPolicyAdjustmentModifier>
            {
                new PercentLenientPolicyAdjustmentAdjuster(-5.7),
                new MinimumLenientPolicyAdjustmentAdjuster(41.9),
                new TieredLenientPolicyAdjustmentAdjuster(
                    new List<double> { 383.0, 2501.0, 5462.0 },
                    new List<double> { 0.86, 0.98, 0.81 }),
            },
            new List<string>(),
            4.2,
            80704.4);
    }

    public double Compute(LenientPolicyAdjustment item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientPolicyAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
