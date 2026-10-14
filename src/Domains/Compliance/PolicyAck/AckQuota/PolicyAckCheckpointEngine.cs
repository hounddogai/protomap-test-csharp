namespace Acme.Workforce.Compliance.PolicyAck.AckQuota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack checkpoint calculation.</summary>
public interface IPolicyAckCheckpointModifier
{
    string Describe();
    double Adjust(PolicyAckCheckpoint item, double valueValue);
}

public sealed record MinimumPolicyAckCheckpointAdjuster(double Minimum) : IPolicyAckCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PolicyAckCheckpointCeilingOffsetAdjuster(double Weight) : IPolicyAckCheckpointModifier
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(PolicyAckCheckpoint item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record FlaggedPolicyAckCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPolicyAckCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckCheckpoint item, double valueValue)
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

/// <summary>Runs the policy ack checkpoint steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckCheckpointEngine(List<IPolicyAckCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckCheckpointEngine DefaultPolicyAckCheckpointEngine(Registry flags)
    {
        return new PolicyAckCheckpointEngine(
            new List<IPolicyAckCheckpointModifier>
            {
                new MinimumPolicyAckCheckpointAdjuster(23.3),
                new PolicyAckCheckpointCeilingOffsetAdjuster(1.71),
                new FlaggedPolicyAckCheckpointAdjuster(flags, "policy-ack-checkpoint-legacy", -0.1),
                new TieredPolicyAckCheckpointAdjuster(
                    new List<double> { 3620.0, 9501.0 },
                    new List<double> { 0.86, 0.97 }),
            },
            new List<string>(),
            3.5,
            39088.3);
    }

    public double Compute(PolicyAckCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
