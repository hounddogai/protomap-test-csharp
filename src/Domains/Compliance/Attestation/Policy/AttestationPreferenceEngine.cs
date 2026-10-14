namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation preference calculation.</summary>
public interface IAttestationPreferenceAdjuster
{
    string Describe();
    double Adjust(AttestationPreference item, double valueValue);
}

public sealed record MinimumAttestationPreferenceAdjuster(double Minimum) : IAttestationPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAttestationPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IAttestationPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAttestationPreferenceAdjuster(double Step) : IAttestationPreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAttestationPreferenceAdjuster(double Percent) : IAttestationPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AttestationPreferenceQuantityOffsetAdjuster(double Weight) : IAttestationPreferenceAdjuster
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(AttestationPreference item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

/// <summary>Runs the attestation preference steps in order and keeps a trace of each.</summary>
public sealed class AttestationPreferenceEngine(List<IAttestationPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationPreferenceEngine DefaultAttestationPreferenceEngine(Registry flags)
    {
        return new AttestationPreferenceEngine(
            new List<IAttestationPreferenceAdjuster>
            {
                new MinimumAttestationPreferenceAdjuster(40.0),
                new FlaggedAttestationPreferenceAdjuster(flags, "attestation-preference-weighted", -7.8),
                new RoundingAttestationPreferenceAdjuster(0.25),
                new PercentAttestationPreferenceAdjuster(6.1),
                new AttestationPreferenceQuantityOffsetAdjuster(1.65),
            },
            new List<string>(),
            4.6,
            87909.1);
    }

    public double Run(AttestationPreference item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
