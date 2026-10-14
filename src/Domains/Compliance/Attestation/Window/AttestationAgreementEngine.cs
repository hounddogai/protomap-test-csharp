namespace Acme.Workforce.Compliance.Attestation.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation agreement calculation.</summary>
public interface IAttestationAgreementStep
{
    string Describe();
    double Adjust(AttestationAgreement item, double valueValue);
}

public sealed record FlaggedAttestationAgreementAdjuster(Registry Flags, string Flag, double Percent) : IAttestationAgreementStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAttestationAgreementAdjuster(double Step) : IAttestationAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAttestationAgreementAdjuster(double Minimum) : IAttestationAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAttestationAgreementAdjuster(double Percent) : IAttestationAgreementStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationAgreementAdjuster(List<double> Bounds, List<double> Rates) : IAttestationAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationAgreement item, double valueValue)
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

public sealed record CapAttestationAgreementAdjuster(double Cap) : IAttestationAgreementStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the attestation agreement steps in order and keeps a trace of each.</summary>
public sealed class AttestationAgreementEngine(List<IAttestationAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationAgreementEngine DefaultAttestationAgreementEngine(Registry flags)
    {
        return new AttestationAgreementEngine(
            new List<IAttestationAgreementStep>
            {
                new FlaggedAttestationAgreementAdjuster(flags, "attestation-agreement-strict", -2.0),
                new RoundingAttestationAgreementAdjuster(0.05),
                new MinimumAttestationAgreementAdjuster(11.6),
                new PercentAttestationAgreementAdjuster(7.9),
                new TieredAttestationAgreementAdjuster(
                    new List<double> { 3845.0, 7620.0, 9679.0 },
                    new List<double> { 1.16, 1.04, 1.01 }),
                new CapAttestationAgreementAdjuster(22800.0),
            },
            new List<string>(),
            1.9,
            42506.1);
    }

    public double Price(AttestationAgreement item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
