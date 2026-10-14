namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy proposal calculation.</summary>
public interface IPolicyProposalFactor
{
    string Describe();
    double Adjust(PolicyProposal item, double valueValue);
}

public sealed record PolicyProposalMarginOffsetAdjuster(double Weight) : IPolicyProposalFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(PolicyProposal item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record RoundingPolicyProposalAdjuster(double Step) : IPolicyProposalFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NamePolicyProposalAdjuster(Dictionary<string, double> Factors) : IPolicyProposalFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPolicyProposalAdjuster(double Percent) : IPolicyProposalFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPolicyProposalAdjuster(double Cap) : IPolicyProposalFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy proposal steps in order and keeps a trace of each.</summary>
public sealed class PolicyProposalEngine(List<IPolicyProposalFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyProposalFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyProposalEngine DefaultPolicyProposalEngine()
    {
        return new PolicyProposalEngine(
            new List<IPolicyProposalFactor>
            {
                new PolicyProposalMarginOffsetAdjuster(0.38),
                new RoundingPolicyProposalAdjuster(1.0),
                new NamePolicyProposalAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.95, ["south"] = 1.06, ["east"] = 1.25 }),
                new PercentPolicyProposalAdjuster(-4.5),
                new CapPolicyProposalAdjuster(13078.0),
            },
            new List<string>(),
            0.2,
            73773.2);
    }

    public double Compute(PolicyProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
