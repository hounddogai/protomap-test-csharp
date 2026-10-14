namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted policy ack proposal calculation.</summary>
public interface IWeightedPolicyAckProposalStep
{
    string Describe();
    double Adjust(WeightedPolicyAckProposal item, double valueValue);
}

public sealed record TitleWeightedPolicyAckProposalAdjuster(Dictionary<string, double> Factors) : IWeightedPolicyAckProposalStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(WeightedPolicyAckProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentWeightedPolicyAckProposalAdjuster(double Percent) : IWeightedPolicyAckProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedPolicyAckProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedWeightedPolicyAckProposalAdjuster(Registry Flags, string Flag, double Percent) : IWeightedPolicyAckProposalStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedPolicyAckProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWeightedPolicyAckProposalAdjuster(List<double> Bounds, List<double> Rates) : IWeightedPolicyAckProposalStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedPolicyAckProposal item, double valueValue)
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

/// <summary>Runs the weighted policy ack proposal steps in order and keeps a trace of each.</summary>
public sealed class WeightedPolicyAckProposalEngine(List<IWeightedPolicyAckProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedPolicyAckProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedPolicyAckProposalEngine DefaultWeightedPolicyAckProposalEngine(Registry flags)
    {
        return new WeightedPolicyAckProposalEngine(
            new List<IWeightedPolicyAckProposalStep>
            {
                new TitleWeightedPolicyAckProposalAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.06, ["retail"] = 1.06 }),
                new PercentWeightedPolicyAckProposalAdjuster(23.6),
                new FlaggedWeightedPolicyAckProposalAdjuster(flags, "weighted-policy-ack-proposal-primary", -4.5),
                new TieredWeightedPolicyAckProposalAdjuster(
                    new List<double> { 5085.0, 7335.0 },
                    new List<double> { 1.02, 1.09 }),
            },
            new List<string>(),
            4.5,
            71248.9);
    }

    public double Evaluate(WeightedPolicyAckProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedPolicyAckProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
