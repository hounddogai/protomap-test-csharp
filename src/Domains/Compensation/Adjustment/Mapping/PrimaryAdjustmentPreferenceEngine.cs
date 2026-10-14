namespace Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>One step of the primary adjustment preference calculation.</summary>
public interface IPrimaryAdjustmentPreferenceStep
{
    string Describe();
    double Adjust(PrimaryAdjustmentPreference item, double valueValue);
}

public sealed record TieredPrimaryAdjustmentPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryAdjustmentPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryAdjustmentPreference item, double valueValue)
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

public sealed record PercentPrimaryAdjustmentPreferenceAdjuster(double Percent) : IPrimaryAdjustmentPreferenceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAdjustmentPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimaryAdjustmentPreferenceAdjuster(double Minimum) : IPrimaryAdjustmentPreferenceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryAdjustmentPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPrimaryAdjustmentPreferenceAdjuster(double Cap) : IPrimaryAdjustmentPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryAdjustmentPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PrimaryAdjustmentPreferenceAmountOffsetAdjuster(double Weight) : IPrimaryAdjustmentPreferenceStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(PrimaryAdjustmentPreference item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the primary adjustment preference steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAdjustmentPreferenceEngine(List<IPrimaryAdjustmentPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAdjustmentPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAdjustmentPreferenceEngine DefaultPrimaryAdjustmentPreferenceEngine()
    {
        return new PrimaryAdjustmentPreferenceEngine(
            new List<IPrimaryAdjustmentPreferenceStep>
            {
                new TieredPrimaryAdjustmentPreferenceAdjuster(
                    new List<double> { 1407.0, 3228.0, 4454.0, 8325.0 },
                    new List<double> { 1.15, 1.19, 0.88, 0.82 }),
                new PercentPrimaryAdjustmentPreferenceAdjuster(-8.3),
                new MinimumPrimaryAdjustmentPreferenceAdjuster(23.3),
                new CapPrimaryAdjustmentPreferenceAdjuster(48264.0),
                new PrimaryAdjustmentPreferenceAmountOffsetAdjuster(1.05),
            },
            new List<string>(),
            3.3,
            19608.4);
    }

    public double Run(PrimaryAdjustmentPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAdjustmentPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
