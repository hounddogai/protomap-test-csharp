namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the attestation threshold calculation.</summary>
public interface IAttestationThresholdAdjuster
{
    string Describe();
    double Adjust(AttestationThreshold item, double valueValue);
}

public sealed record PercentAttestationThresholdAdjuster(double Percent) : IAttestationThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAttestationThresholdAdjuster(Registry Flags, string Flag, double Percent) : IAttestationThresholdAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AttestationThreshold item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationThresholdAdjuster(List<double> Bounds, List<double> Rates) : IAttestationThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationThreshold item, double valueValue)
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

/// <summary>Runs the attestation threshold steps in order and keeps a trace of each.</summary>
public sealed class AttestationThresholdEngine(List<IAttestationThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationThresholdEngine DefaultAttestationThresholdEngine(Registry flags)
    {
        return new AttestationThresholdEngine(
            new List<IAttestationThresholdAdjuster>
            {
                new PercentAttestationThresholdAdjuster(-24.5),
                new FlaggedAttestationThresholdAdjuster(flags, "attestation-threshold-legacy", 8.9),
                new TieredAttestationThresholdAdjuster(
                    new List<double> { 2936.0, 4592.0, 9151.0 },
                    new List<double> { 1.2, 0.93, 1.08 }),
            },
            new List<string>(),
            2.7,
            2270.7);
    }

    public double Evaluate(AttestationThreshold item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
