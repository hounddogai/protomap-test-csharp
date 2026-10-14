namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation policy calculation.</summary>
public interface IAttestationPolicyAdjuster
{
    string Describe();
    double Adjust(AttestationPolicy item, double valueValue);
}

public sealed record UnitAttestationPolicyAdjuster(Dictionary<string, double> Factors) : IAttestationPolicyAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AttestationPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAttestationPolicyAdjuster(double Percent) : IAttestationPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationPolicyAdjuster(List<double> Bounds, List<double> Rates) : IAttestationPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationPolicy item, double valueValue)
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

/// <summary>Runs the attestation policy steps in order and keeps a trace of each.</summary>
public sealed class AttestationPolicyEngine(List<IAttestationPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationPolicyEngine DefaultAttestationPolicyEngine()
    {
        return new AttestationPolicyEngine(
            new List<IAttestationPolicyAdjuster>
            {
                new UnitAttestationPolicyAdjuster(new Dictionary<string, double> { ["east"] = 1.34 }),
                new PercentAttestationPolicyAdjuster(20.7),
                new TieredAttestationPolicyAdjuster(
                    new List<double> { 1505.0, 1522.0, 2804.0 },
                    new List<double> { 1.15, 1.1, 1.06 }),
            },
            new List<string>(),
            3.6,
            55126.5);
    }

    public double Evaluate(AttestationPolicy item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
