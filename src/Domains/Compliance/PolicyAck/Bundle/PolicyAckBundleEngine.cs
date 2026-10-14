namespace Acme.Workforce.Compliance.PolicyAck.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack bundle calculation.</summary>
public interface IPolicyAckBundleFactor
{
    string Describe();
    double Adjust(PolicyAckBundle item, double valueValue);
}

public sealed record PolicyAckBundleExpiresAtOffsetAdjuster(double Weight) : IPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(PolicyAckBundle item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record FlaggedPolicyAckBundleAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPolicyAckBundleAdjuster(double Minimum) : IPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPolicyAckBundleAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckBundle item, double valueValue)
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

public sealed record CapPolicyAckBundleAdjuster(double Cap) : IPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy ack bundle steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckBundleEngine(List<IPolicyAckBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckBundleEngine DefaultPolicyAckBundleEngine(Registry flags)
    {
        return new PolicyAckBundleEngine(
            new List<IPolicyAckBundleFactor>
            {
                new PolicyAckBundleExpiresAtOffsetAdjuster(1.19),
                new FlaggedPolicyAckBundleAdjuster(flags, "policy-ack-bundle-pending", -4.2),
                new MinimumPolicyAckBundleAdjuster(37.4),
                new TieredPolicyAckBundleAdjuster(
                    new List<double> { 361.0, 2073.0 },
                    new List<double> { 0.93, 0.99 }),
                new CapPolicyAckBundleAdjuster(25999.0),
            },
            new List<string>(),
            0.5,
            1332.5);
    }

    public double Run(PolicyAckBundle item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
