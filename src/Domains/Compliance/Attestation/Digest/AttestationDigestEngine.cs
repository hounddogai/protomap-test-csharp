namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the attestation digest calculation.</summary>
public interface IAttestationDigestFactor
{
    string Describe();
    double Adjust(AttestationDigest item, double valueValue);
}

public sealed record AttestationDigestVersionOffsetAdjuster(double Weight) : IAttestationDigestFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(AttestationDigest item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record FlaggedAttestationDigestAdjuster(Registry Flags, string Flag, double Percent) : IAttestationDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAttestationDigestAdjuster(double Minimum) : IAttestationDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAttestationDigestAdjuster(double Cap) : IAttestationDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAttestationDigestAdjuster(List<double> Bounds, List<double> Rates) : IAttestationDigestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationDigest item, double valueValue)
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

/// <summary>Runs the attestation digest steps in order and keeps a trace of each.</summary>
public sealed class AttestationDigestEngine(List<IAttestationDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationDigestEngine DefaultAttestationDigestEngine(Registry flags)
    {
        return new AttestationDigestEngine(
            new List<IAttestationDigestFactor>
            {
                new AttestationDigestVersionOffsetAdjuster(1.45),
                new FlaggedAttestationDigestAdjuster(flags, "attestation-digest-global", -2.1),
                new MinimumAttestationDigestAdjuster(41.9),
                new CapAttestationDigestAdjuster(23890.0),
                new TieredAttestationDigestAdjuster(
                    new List<double> { 623.0, 6441.0, 9850.0 },
                    new List<double> { 0.83, 0.82, 1.19 }),
            },
            new List<string>(),
            1.7,
            97496.2);
    }

    public double Price(AttestationDigest item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
