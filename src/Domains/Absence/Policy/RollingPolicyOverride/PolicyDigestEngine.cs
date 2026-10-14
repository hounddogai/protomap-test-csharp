namespace Acme.Workforce.Absence.Policy.RollingPolicyOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy digest calculation.</summary>
public interface IPolicyDigestModifier
{
    string Describe();
    double Adjust(PolicyDigest item, double valueValue);
}

public sealed record PercentPolicyDigestAdjuster(double Percent) : IPolicyDigestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionPolicyDigestAdjuster(Dictionary<string, double> Factors) : IPolicyDigestModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(PolicyDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPolicyDigestAdjuster(Registry Flags, string Flag, double Percent) : IPolicyDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy digest steps in order and keeps a trace of each.</summary>
public sealed class PolicyDigestEngine(List<IPolicyDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyDigestEngine DefaultPolicyDigestEngine(Registry flags)
    {
        return new PolicyDigestEngine(
            new List<IPolicyDigestModifier>
            {
                new PercentPolicyDigestAdjuster(8.0),
                new RegionPolicyDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.83, ["north"] = 1.15 }),
                new FlaggedPolicyDigestAdjuster(flags, "policy-digest-weighted", -6.1),
            },
            new List<string>(),
            3.5,
            84845.0);
    }

    public double Run(PolicyDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
