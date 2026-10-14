namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation override calculation.</summary>
public interface IAttestationOverrideStep
{
    string Describe();
    double Adjust(AttestationOverride item, double valueValue);
}

public sealed record TieredAttestationOverrideAdjuster(List<double> Bounds, List<double> Rates) : IAttestationOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationOverride item, double valueValue)
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

public sealed record RoundingAttestationOverrideAdjuster(double Step) : IAttestationOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAttestationOverrideAdjuster(double Minimum) : IAttestationOverrideStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the attestation override steps in order and keeps a trace of each.</summary>
public sealed class AttestationOverrideEngine(List<IAttestationOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationOverrideEngine DefaultAttestationOverrideEngine()
    {
        return new AttestationOverrideEngine(
            new List<IAttestationOverrideStep>
            {
                new TieredAttestationOverrideAdjuster(
                    new List<double> { 4667.0, 5273.0, 6972.0, 8092.0 },
                    new List<double> { 1.15, 0.85, 0.95, 0.93 }),
                new RoundingAttestationOverrideAdjuster(5.0),
                new MinimumAttestationOverrideAdjuster(41.0),
            },
            new List<string>(),
            3.9,
            82203.0);
    }

    public double Run(AttestationOverride item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
