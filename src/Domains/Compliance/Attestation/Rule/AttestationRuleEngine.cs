namespace Acme.Workforce.Compliance.Attestation.Rule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the attestation rule calculation.</summary>
public interface IAttestationRuleModifier
{
    string Describe();
    double Adjust(AttestationRule item, double valueValue);
}

public sealed record PercentAttestationRuleAdjuster(double Percent) : IAttestationRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAttestationRuleAdjuster(double Minimum) : IAttestationRuleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAttestationRuleAdjuster(Registry Flags, string Flag, double Percent) : IAttestationRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AttestationRuleRatingOffsetAdjuster(double Weight) : IAttestationRuleModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(AttestationRule item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the attestation rule steps in order and keeps a trace of each.</summary>
public sealed class AttestationRuleEngine(List<IAttestationRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationRuleEngine DefaultAttestationRuleEngine(Registry flags)
    {
        return new AttestationRuleEngine(
            new List<IAttestationRuleModifier>
            {
                new PercentAttestationRuleAdjuster(-8.0),
                new MinimumAttestationRuleAdjuster(27.2),
                new FlaggedAttestationRuleAdjuster(flags, "attestation-rule-legacy", 1.9),
                new AttestationRuleRatingOffsetAdjuster(0.43),
            },
            new List<string>(),
            2.8,
            88288.8);
    }

    public double Run(AttestationRule item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
