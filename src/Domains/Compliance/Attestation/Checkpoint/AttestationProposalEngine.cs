namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation proposal calculation.</summary>
public interface IAttestationProposalModifier
{
    string Describe();
    double Adjust(AttestationProposal item, double valueValue);
}

public sealed record MinimumAttestationProposalAdjuster(double Minimum) : IAttestationProposalModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AttestationProposalWeightOffsetAdjuster(double Weight) : IAttestationProposalModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(AttestationProposal item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CapAttestationProposalAdjuster(double Cap) : IAttestationProposalModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleAttestationProposalAdjuster(Dictionary<string, double> Factors) : IAttestationProposalModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AttestationProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the attestation proposal steps in order and keeps a trace of each.</summary>
public sealed class AttestationProposalEngine(List<IAttestationProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationProposalEngine DefaultAttestationProposalEngine()
    {
        return new AttestationProposalEngine(
            new List<IAttestationProposalModifier>
            {
                new MinimumAttestationProposalAdjuster(49.9),
                new AttestationProposalWeightOffsetAdjuster(1.53),
                new CapAttestationProposalAdjuster(19430.0),
                new TitleAttestationProposalAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.94, ["south"] = 1.3, ["west"] = 0.78 }),
            },
            new List<string>(),
            4.3,
            54530.0);
    }

    public double Compute(AttestationProposal item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
