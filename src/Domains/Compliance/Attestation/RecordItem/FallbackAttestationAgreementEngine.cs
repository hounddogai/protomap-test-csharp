namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback attestation agreement calculation.</summary>
public interface IFallbackAttestationAgreementAdjuster
{
    string Describe();
    double Adjust(FallbackAttestationAgreement item, double valueValue);
}

public sealed record PercentFallbackAttestationAgreementAdjuster(double Percent) : IFallbackAttestationAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackAttestationAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFallbackAttestationAgreementAdjuster(Registry Flags, string Flag, double Percent) : IFallbackAttestationAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackAttestationAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFallbackAttestationAgreementAdjuster(double Cap) : IFallbackAttestationAgreementAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackAttestationAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FallbackAttestationAgreementExpiresAtOffsetAdjuster(double Weight) : IFallbackAttestationAgreementAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(FallbackAttestationAgreement item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record TieredFallbackAttestationAgreementAdjuster(List<double> Bounds, List<double> Rates) : IFallbackAttestationAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackAttestationAgreement item, double valueValue)
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

public sealed record RoundingFallbackAttestationAgreementAdjuster(double Step) : IFallbackAttestationAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackAttestationAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the fallback attestation agreement steps in order and keeps a trace of each.</summary>
public sealed class FallbackAttestationAgreementEngine(List<IFallbackAttestationAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAttestationAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAttestationAgreementEngine DefaultFallbackAttestationAgreementEngine(Registry flags)
    {
        return new FallbackAttestationAgreementEngine(
            new List<IFallbackAttestationAgreementAdjuster>
            {
                new PercentFallbackAttestationAgreementAdjuster(10.9),
                new FlaggedFallbackAttestationAgreementAdjuster(flags, "fallback-attestation-agreement-manual", -9.9),
                new CapFallbackAttestationAgreementAdjuster(41044.0),
                new FallbackAttestationAgreementExpiresAtOffsetAdjuster(1.41),
                new TieredFallbackAttestationAgreementAdjuster(
                    new List<double> { 730.0, 7633.0 },
                    new List<double> { 1.07, 1.04 }),
                new RoundingFallbackAttestationAgreementAdjuster(1.0),
            },
            new List<string>(),
            2.7,
            95238.6);
    }

    public double Evaluate(FallbackAttestationAgreement item)
    {
        var current = item.ProjectedVolume();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<FallbackAttestationAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
