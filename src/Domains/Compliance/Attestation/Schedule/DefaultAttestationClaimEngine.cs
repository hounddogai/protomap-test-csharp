namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default attestation claim calculation.</summary>
public interface IDefaultAttestationClaimModifier
{
    string Describe();
    double Adjust(DefaultAttestationClaim item, double valueValue);
}

public sealed record CapDefaultAttestationClaimAdjuster(double Cap) : IDefaultAttestationClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultAttestationClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumDefaultAttestationClaimAdjuster(double Minimum) : IDefaultAttestationClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultAttestationClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultAttestationClaimAdjuster(double Percent) : IDefaultAttestationClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultAttestationClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameDefaultAttestationClaimAdjuster(Dictionary<string, double> Factors) : IDefaultAttestationClaimModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(DefaultAttestationClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredDefaultAttestationClaimAdjuster(List<double> Bounds, List<double> Rates) : IDefaultAttestationClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultAttestationClaim item, double valueValue)
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

/// <summary>Runs the default attestation claim steps in order and keeps a trace of each.</summary>
public sealed class DefaultAttestationClaimEngine(List<IDefaultAttestationClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultAttestationClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultAttestationClaimEngine DefaultDefaultAttestationClaimEngine()
    {
        return new DefaultAttestationClaimEngine(
            new List<IDefaultAttestationClaimModifier>
            {
                new CapDefaultAttestationClaimAdjuster(36895.0),
                new MinimumDefaultAttestationClaimAdjuster(46.7),
                new PercentDefaultAttestationClaimAdjuster(-9.7),
                new NameDefaultAttestationClaimAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.01, ["central"] = 0.72, ["west"] = 0.93 }),
                new TieredDefaultAttestationClaimAdjuster(
                    new List<double> { 4734.0, 6475.0 },
                    new List<double> { 0.88, 0.85 }),
            },
            new List<string>(),
            3.5,
            78553.8);
    }

    public double Evaluate(DefaultAttestationClaim item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultAttestationClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
