namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the nomination preference calculation.</summary>
public interface INominationPreferenceModifier
{
    string Describe();
    double Adjust(NominationPreference item, double valueValue);
}

public sealed record NominationPreferenceVersionOffsetAdjuster(double Weight) : INominationPreferenceModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(NominationPreference item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record CapNominationPreferenceAdjuster(double Cap) : INominationPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredNominationPreferenceAdjuster(List<double> Bounds, List<double> Rates) : INominationPreferenceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationPreference item, double valueValue)
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

public sealed record FlaggedNominationPreferenceAdjuster(Registry Flags, string Flag, double Percent) : INominationPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the nomination preference steps in order and keeps a trace of each.</summary>
public sealed class NominationPreferenceEngine(List<INominationPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationPreferenceEngine DefaultNominationPreferenceEngine(Registry flags)
    {
        return new NominationPreferenceEngine(
            new List<INominationPreferenceModifier>
            {
                new NominationPreferenceVersionOffsetAdjuster(1.0),
                new CapNominationPreferenceAdjuster(2752.0),
                new TieredNominationPreferenceAdjuster(
                    new List<double> { 906.0, 3806.0, 9514.0 },
                    new List<double> { 0.99, 1.13, 1.05 }),
                new FlaggedNominationPreferenceAdjuster(flags, "nomination-preference-weighted", -6.6),
            },
            new List<string>(),
            2.2,
            28663.4);
    }

    public double Compute(NominationPreference item)
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

    public Dictionary<string, double> EvaluateAll(List<NominationPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
