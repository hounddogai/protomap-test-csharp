namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack cycle calculation.</summary>
public interface IPolicyAckCycleFactor
{
    string Describe();
    double Adjust(PolicyAckCycle item, double valueValue);
}

public sealed record CapPolicyAckCycleAdjuster(double Cap) : IPolicyAckCycleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPolicyAckCycleAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckCycleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPolicyAckCycleAdjuster(double Minimum) : IPolicyAckCycleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RegionPolicyAckCycleAdjuster(Dictionary<string, double> Factors) : IPolicyAckCycleFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PolicyAckCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPolicyAckCycleAdjuster(double Percent) : IPolicyAckCycleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy ack cycle steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckCycleEngine(List<IPolicyAckCycleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckCycleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckCycleEngine DefaultPolicyAckCycleEngine(Registry flags)
    {
        return new PolicyAckCycleEngine(
            new List<IPolicyAckCycleFactor>
            {
                new CapPolicyAckCycleAdjuster(22613.0),
                new FlaggedPolicyAckCycleAdjuster(flags, "policy-ack-cycle-legacy", 0.9),
                new MinimumPolicyAckCycleAdjuster(7.6),
                new RegionPolicyAckCycleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.1, ["retail"] = 1.39, ["central"] = 1.34 }),
                new PercentPolicyAckCycleAdjuster(21.1),
            },
            new List<string>(),
            0.8,
            66086.6);
    }

    public double Evaluate(PolicyAckCycle item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
