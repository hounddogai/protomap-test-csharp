namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy claim calculation.</summary>
public interface IPolicyClaimModifier
{
    string Describe();
    double Adjust(PolicyClaim item, double valueValue);
}

public sealed record RoundingPolicyClaimAdjuster(double Step) : IPolicyClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryPolicyClaimAdjuster(Dictionary<string, double> Factors) : IPolicyClaimModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PolicyClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyClaimAdjuster(double Cap) : IPolicyClaimModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPolicyClaimAdjuster(double Percent) : IPolicyClaimModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy claim steps in order and keeps a trace of each.</summary>
public sealed class PolicyClaimEngine(List<IPolicyClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyClaimEngine DefaultPolicyClaimEngine()
    {
        return new PolicyClaimEngine(
            new List<IPolicyClaimModifier>
            {
                new RoundingPolicyClaimAdjuster(0.5),
                new CategoryPolicyClaimAdjuster(new Dictionary<string, double> { ["east"] = 1.06 }),
                new CapPolicyClaimAdjuster(12325.0),
                new PercentPolicyClaimAdjuster(10.2),
            },
            new List<string>(),
            4.2,
            62979.4);
    }

    public double Run(PolicyClaim item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
