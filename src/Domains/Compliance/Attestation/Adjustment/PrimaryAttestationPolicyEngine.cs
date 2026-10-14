namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary attestation policy calculation.</summary>
public interface IPrimaryAttestationPolicyModifier
{
    string Describe();
    double Adjust(PrimaryAttestationPolicy item, double valueValue);
}

public sealed record TieredPrimaryAttestationPolicyAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryAttestationPolicyModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryAttestationPolicy item, double valueValue)
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

public sealed record CapPrimaryAttestationPolicyAdjuster(double Cap) : IPrimaryAttestationPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryAttestationPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPrimaryAttestationPolicyAdjuster(double Step) : IPrimaryAttestationPolicyModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryAttestationPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryAttestationPolicyAdjuster(double Percent) : IPrimaryAttestationPolicyModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAttestationPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the primary attestation policy steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAttestationPolicyEngine(List<IPrimaryAttestationPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAttestationPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAttestationPolicyEngine DefaultPrimaryAttestationPolicyEngine()
    {
        return new PrimaryAttestationPolicyEngine(
            new List<IPrimaryAttestationPolicyModifier>
            {
                new TieredPrimaryAttestationPolicyAdjuster(
                    new List<double> { 4399.0, 7750.0, 8953.0 },
                    new List<double> { 1.01, 0.91, 0.81 }),
                new CapPrimaryAttestationPolicyAdjuster(10683.0),
                new RoundingPrimaryAttestationPolicyAdjuster(1.0),
                new PercentPrimaryAttestationPolicyAdjuster(-5.5),
            },
            new List<string>(),
            4.8,
            55786.9);
    }

    public double Evaluate(PrimaryAttestationPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAttestationPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
