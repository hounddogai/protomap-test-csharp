namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback attestation summary calculation.</summary>
public interface IFallbackAttestationSummaryModifier
{
    string Describe();
    double Adjust(FallbackAttestationSummary item, double valueValue);
}

public sealed record TieredFallbackAttestationSummaryAdjuster(List<double> Bounds, List<double> Rates) : IFallbackAttestationSummaryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackAttestationSummary item, double valueValue)
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

public sealed record FlaggedFallbackAttestationSummaryAdjuster(Registry Flags, string Flag, double Percent) : IFallbackAttestationSummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackAttestationSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFallbackAttestationSummaryAdjuster(double Percent) : IFallbackAttestationSummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackAttestationSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback attestation summary steps in order and keeps a trace of each.</summary>
public sealed class FallbackAttestationSummaryEngine(List<IFallbackAttestationSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAttestationSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAttestationSummaryEngine DefaultFallbackAttestationSummaryEngine(Registry flags)
    {
        return new FallbackAttestationSummaryEngine(
            new List<IFallbackAttestationSummaryModifier>
            {
                new TieredFallbackAttestationSummaryAdjuster(
                    new List<double> { 6481.0, 6881.0, 7327.0 },
                    new List<double> { 1.15, 0.97, 1.13 }),
                new FlaggedFallbackAttestationSummaryAdjuster(flags, "fallback-attestation-summary-pending", -4.7),
                new PercentFallbackAttestationSummaryAdjuster(-3.3),
            },
            new List<string>(),
            4.0,
            90970.6);
    }

    public double Evaluate(FallbackAttestationSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAttestationSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
