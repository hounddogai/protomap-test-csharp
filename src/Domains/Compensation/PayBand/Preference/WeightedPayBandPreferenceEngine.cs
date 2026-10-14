namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted pay band preference calculation.</summary>
public interface IWeightedPayBandPreferenceModifier
{
    string Describe();
    double Adjust(WeightedPayBandPreference item, double valueValue);
}

public sealed record CapWeightedPayBandPreferenceAdjuster(double Cap) : IWeightedPayBandPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedPayBandPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingWeightedPayBandPreferenceAdjuster(double Step) : IWeightedPayBandPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedPayBandPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredWeightedPayBandPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPayBandPreferenceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPayBandPreference item, double valueValue)
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

public sealed record MinimumWeightedPayBandPreferenceAdjuster(double Minimum) : IWeightedPayBandPreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedPayBandPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted pay band preference steps in order and keeps a trace of each.</summary>
public sealed class WeightedPayBandPreferenceEngine(List<IWeightedPayBandPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPayBandPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPayBandPreferenceEngine DefaultWeightedPayBandPreferenceEngine()
    {
        return new WeightedPayBandPreferenceEngine(
            new List<IWeightedPayBandPreferenceModifier>
            {
                new CapWeightedPayBandPreferenceAdjuster(30146.0),
                new RoundingWeightedPayBandPreferenceAdjuster(0.1),
                new TieredWeightedPayBandPreferenceAdjuster(
                    new List<double> { 3061.0, 7457.0 },
                    new List<double> { 0.84, 1.08 }),
                new MinimumWeightedPayBandPreferenceAdjuster(21.5),
            },
            new List<string>(),
            0.7,
            97026.5);
    }

    public double Compute(WeightedPayBandPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPayBandPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
