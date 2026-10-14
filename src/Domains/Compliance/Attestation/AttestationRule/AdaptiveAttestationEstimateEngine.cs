namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive attestation estimate calculation.</summary>
public interface IAdaptiveAttestationEstimateModifier
{
    string Describe();
    double Adjust(AdaptiveAttestationEstimate item, double valueValue);
}

public sealed record FlaggedAdaptiveAttestationEstimateAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveAttestationEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveAttestationEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveAttestationEstimateAdjuster(double Minimum) : IAdaptiveAttestationEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveAttestationEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record UnitAdaptiveAttestationEstimateAdjuster(Dictionary<string, double> Factors) : IAdaptiveAttestationEstimateModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AdaptiveAttestationEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdaptiveAttestationEstimateAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveAttestationEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveAttestationEstimate item, double valueValue)
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

public sealed record AdaptiveAttestationEstimateTenureMonthsOffsetAdjuster(double Weight) : IAdaptiveAttestationEstimateModifier
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(AdaptiveAttestationEstimate item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

/// <summary>Runs the adaptive attestation estimate steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveAttestationEstimateEngine(List<IAdaptiveAttestationEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveAttestationEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveAttestationEstimateEngine DefaultAdaptiveAttestationEstimateEngine(Registry flags)
    {
        return new AdaptiveAttestationEstimateEngine(
            new List<IAdaptiveAttestationEstimateModifier>
            {
                new FlaggedAdaptiveAttestationEstimateAdjuster(flags, "adaptive-attestation-estimate-default", -2.1),
                new MinimumAdaptiveAttestationEstimateAdjuster(7.4),
                new UnitAdaptiveAttestationEstimateAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.01, ["online"] = 1.08, ["north"] = 0.85 }),
                new TieredAdaptiveAttestationEstimateAdjuster(
                    new List<double> { 3240.0, 5603.0, 6807.0, 8893.0 },
                    new List<double> { 1.19, 0.91, 1.09, 0.86 }),
                new AdaptiveAttestationEstimateTenureMonthsOffsetAdjuster(0.53),
            },
            new List<string>(),
            3.2,
            55825.8);
    }

    public double Run(AdaptiveAttestationEstimate item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveAttestationEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
