namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped attestation proposal calculation.</summary>
public interface ICappedAttestationProposalModifier
{
    string Describe();
    double Adjust(CappedAttestationProposal item, double valueValue);
}

public sealed record CapCappedAttestationProposalAdjuster(double Cap) : ICappedAttestationProposalModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAttestationProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedAttestationProposalAdjuster(Registry Flags, string Flag, double Percent) : ICappedAttestationProposalModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAttestationProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredCappedAttestationProposalAdjuster(List<double> Bounds, List<double> Rates) : ICappedAttestationProposalModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedAttestationProposal item, double valueValue)
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

public sealed record CappedAttestationProposalMarginOffsetAdjuster(double Weight) : ICappedAttestationProposalModifier
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(CappedAttestationProposal item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record MinimumCappedAttestationProposalAdjuster(double Minimum) : ICappedAttestationProposalModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedAttestationProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LocaleCappedAttestationProposalAdjuster(Dictionary<string, double> Factors) : ICappedAttestationProposalModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(CappedAttestationProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the capped attestation proposal steps in order and keeps a trace of each.</summary>
public sealed class CappedAttestationProposalEngine(List<ICappedAttestationProposalModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAttestationProposalModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAttestationProposalEngine DefaultCappedAttestationProposalEngine(Registry flags)
    {
        return new CappedAttestationProposalEngine(
            new List<ICappedAttestationProposalModifier>
            {
                new CapCappedAttestationProposalAdjuster(21696.0),
                new FlaggedCappedAttestationProposalAdjuster(flags, "capped-attestation-proposal-strict", -5.9),
                new TieredCappedAttestationProposalAdjuster(
                    new List<double> { 5635.0, 8433.0, 9390.0 },
                    new List<double> { 0.87, 1.04, 1.09 }),
                new CappedAttestationProposalMarginOffsetAdjuster(0.25),
                new MinimumCappedAttestationProposalAdjuster(29.6),
                new LocaleCappedAttestationProposalAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.77, ["east"] = 1.28 }),
            },
            new List<string>(),
            3.9,
            50803.0);
    }

    public double Compute(CappedAttestationProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedAttestationProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
