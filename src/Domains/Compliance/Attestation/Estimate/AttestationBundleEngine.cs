namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the attestation bundle calculation.</summary>
public interface IAttestationBundleAdjuster
{
    string Describe();
    double Adjust(AttestationBundle item, double valueValue);
}

public sealed record MinimumAttestationBundleAdjuster(double Minimum) : IAttestationBundleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAttestationBundleAdjuster(double Cap) : IAttestationBundleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAttestationBundleAdjuster(Registry Flags, string Flag, double Percent) : IAttestationBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationBundleAdjuster(List<double> Bounds, List<double> Rates) : IAttestationBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationBundle item, double valueValue)
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

public sealed record AttestationBundleMaximumOffsetAdjuster(double Weight) : IAttestationBundleAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(AttestationBundle item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the attestation bundle steps in order and keeps a trace of each.</summary>
public sealed class AttestationBundleEngine(List<IAttestationBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationBundleEngine DefaultAttestationBundleEngine(Registry flags)
    {
        return new AttestationBundleEngine(
            new List<IAttestationBundleAdjuster>
            {
                new MinimumAttestationBundleAdjuster(21.6),
                new CapAttestationBundleAdjuster(19592.0),
                new FlaggedAttestationBundleAdjuster(flags, "attestation-bundle-manual", -1.9),
                new TieredAttestationBundleAdjuster(
                    new List<double> { 2012.0, 2821.0, 6277.0 },
                    new List<double> { 1.1, 1.11, 0.88 }),
                new AttestationBundleMaximumOffsetAdjuster(1.3),
            },
            new List<string>(),
            3.1,
            77486.3);
    }

    public double Compute(AttestationBundle item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
