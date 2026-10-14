namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack item calculation.</summary>
public interface IPolicyAckItemModifier
{
    string Describe();
    double Adjust(PolicyAckItem item, double valueValue);
}

public sealed record FlaggedPolicyAckItemAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckItemModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPolicyAckItemAdjuster(double Cap) : IPolicyAckItemModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPolicyAckItemAdjuster(double Percent) : IPolicyAckItemModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy ack item steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckItemEngine(List<IPolicyAckItemModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckItemModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckItemEngine DefaultPolicyAckItemEngine(Registry flags)
    {
        return new PolicyAckItemEngine(
            new List<IPolicyAckItemModifier>
            {
                new FlaggedPolicyAckItemAdjuster(flags, "policy-ack-item-rolling", -5.5),
                new CapPolicyAckItemAdjuster(15394.0),
                new PercentPolicyAckItemAdjuster(-7.0),
            },
            new List<string>(),
            1.8,
            32816.5);
    }

    public double Evaluate(PolicyAckItem item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
