namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation manifest calculation.</summary>
public interface IAttestationManifestFactor
{
    string Describe();
    double Adjust(AttestationManifest item, double valueValue);
}

public sealed record PercentAttestationManifestAdjuster(double Percent) : IAttestationManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelAttestationManifestAdjuster(Dictionary<string, double> Factors) : IAttestationManifestFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AttestationManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAttestationManifestAdjuster(Registry Flags, string Flag, double Percent) : IAttestationManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AttestationManifestVersionOffsetAdjuster(double Weight) : IAttestationManifestFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(AttestationManifest item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record RoundingAttestationManifestAdjuster(double Step) : IAttestationManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAttestationManifestAdjuster(double Minimum) : IAttestationManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the attestation manifest steps in order and keeps a trace of each.</summary>
public sealed class AttestationManifestEngine(List<IAttestationManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationManifestEngine DefaultAttestationManifestEngine(Registry flags)
    {
        return new AttestationManifestEngine(
            new List<IAttestationManifestFactor>
            {
                new PercentAttestationManifestAdjuster(8.0),
                new LabelAttestationManifestAdjuster(new Dictionary<string, double> { ["south"] = 0.85 }),
                new FlaggedAttestationManifestAdjuster(flags, "attestation-manifest-weighted", -3.0),
                new AttestationManifestVersionOffsetAdjuster(1.89),
                new RoundingAttestationManifestAdjuster(0.5),
                new MinimumAttestationManifestAdjuster(14.7),
            },
            new List<string>(),
            1.8,
            56310.9);
    }

    public double Run(AttestationManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
