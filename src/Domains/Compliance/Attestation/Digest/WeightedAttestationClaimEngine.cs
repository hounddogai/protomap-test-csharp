namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted attestation claim calculation.</summary>
public interface IWeightedAttestationClaimStep
{
    string Describe();
    double Adjust(WeightedAttestationClaim item, double valueValue);
}

public sealed record FlaggedWeightedAttestationClaimAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAttestationClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAttestationClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleWeightedAttestationClaimAdjuster(Dictionary<string, double> Factors) : IWeightedAttestationClaimStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WeightedAttestationClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentWeightedAttestationClaimAdjuster(double Percent) : IWeightedAttestationClaimStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedAttestationClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedAttestationClaimAdjuster(double Minimum) : IWeightedAttestationClaimStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAttestationClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the weighted attestation claim steps in order and keeps a trace of each.</summary>
public sealed class WeightedAttestationClaimEngine(List<IWeightedAttestationClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAttestationClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAttestationClaimEngine DefaultWeightedAttestationClaimEngine(Registry flags)
    {
        return new WeightedAttestationClaimEngine(
            new List<IWeightedAttestationClaimStep>
            {
                new FlaggedWeightedAttestationClaimAdjuster(flags, "weighted-attestation-claim-global", -0.7),
                new TitleWeightedAttestationClaimAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.17, ["west"] = 1.36, ["online"] = 1.14 }),
                new PercentWeightedAttestationClaimAdjuster(-11.4),
                new MinimumWeightedAttestationClaimAdjuster(40.4),
            },
            new List<string>(),
            1.6,
            43684.7);
    }

    public double Evaluate(WeightedAttestationClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAttestationClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
