namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack quota calculation.</summary>
public interface IPolicyAckQuotaStep
{
    string Describe();
    double Adjust(PolicyAckQuota item, double valueValue);
}

public sealed record MinimumPolicyAckQuotaAdjuster(double Minimum) : IPolicyAckQuotaStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPolicyAckQuotaAdjuster(double Percent) : IPolicyAckQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPolicyAckQuotaAdjuster(double Cap) : IPolicyAckQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPolicyAckQuotaAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPolicyAckQuotaAdjuster(double Step) : IPolicyAckQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the policy ack quota steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckQuotaEngine(List<IPolicyAckQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckQuotaEngine DefaultPolicyAckQuotaEngine(Registry flags)
    {
        return new PolicyAckQuotaEngine(
            new List<IPolicyAckQuotaStep>
            {
                new MinimumPolicyAckQuotaAdjuster(20.2),
                new PercentPolicyAckQuotaAdjuster(15.0),
                new CapPolicyAckQuotaAdjuster(35832.0),
                new FlaggedPolicyAckQuotaAdjuster(flags, "policy-ack-quota-fallback", -3.8),
                new RoundingPolicyAckQuotaAdjuster(0.1),
            },
            new List<string>(),
            2.7,
            94779.5);
    }

    public double Run(PolicyAckQuota item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PolicyAckQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
