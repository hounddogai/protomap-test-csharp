namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack override calculation.</summary>
public interface IPolicyAckOverrideAdjuster
{
    string Describe();
    double Adjust(PolicyAckOverride item, double valueValue);
}

public sealed record PolicyAckOverrideCeilingOffsetAdjuster(double Weight) : IPolicyAckOverrideAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(PolicyAckOverride item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record ChannelPolicyAckOverrideAdjuster(Dictionary<string, double> Factors) : IPolicyAckOverrideAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PolicyAckOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyAckOverrideAdjuster(double Cap) : IPolicyAckOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPolicyAckOverrideAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPolicyAckOverrideAdjuster(double Percent) : IPolicyAckOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy ack override steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckOverrideEngine(List<IPolicyAckOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckOverrideEngine DefaultPolicyAckOverrideEngine(Registry flags)
    {
        return new PolicyAckOverrideEngine(
            new List<IPolicyAckOverrideAdjuster>
            {
                new PolicyAckOverrideCeilingOffsetAdjuster(1.64),
                new ChannelPolicyAckOverrideAdjuster(new Dictionary<string, double> { ["east"] = 0.91 }),
                new CapPolicyAckOverrideAdjuster(15285.0),
                new FlaggedPolicyAckOverrideAdjuster(flags, "policy-ack-override-manual", 2.9),
                new PercentPolicyAckOverrideAdjuster(3.2),
            },
            new List<string>(),
            1.9,
            79556.6);
    }

    public double Compute(PolicyAckOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
