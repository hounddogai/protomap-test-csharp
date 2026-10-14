namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling policy override calculation.</summary>
public interface IRollingPolicyOverrideFactor
{
    string Describe();
    double Adjust(RollingPolicyOverride item, double valueValue);
}

public sealed record MinimumRollingPolicyOverrideAdjuster(double Minimum) : IRollingPolicyOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingPolicyOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRollingPolicyOverrideAdjuster(double Percent) : IRollingPolicyOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingPolicyOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingPolicyOverrideAdjuster(double Cap) : IRollingPolicyOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPolicyOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryRollingPolicyOverrideAdjuster(Dictionary<string, double> Factors) : IRollingPolicyOverrideFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RollingPolicyOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRollingPolicyOverrideAdjuster(Registry Flags, string Flag, double Percent) : IRollingPolicyOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPolicyOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling policy override steps in order and keeps a trace of each.</summary>
public sealed class RollingPolicyOverrideEngine(List<IRollingPolicyOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPolicyOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPolicyOverrideEngine DefaultRollingPolicyOverrideEngine(Registry flags)
    {
        return new RollingPolicyOverrideEngine(
            new List<IRollingPolicyOverrideFactor>
            {
                new MinimumRollingPolicyOverrideAdjuster(35.6),
                new PercentRollingPolicyOverrideAdjuster(20.8),
                new CapRollingPolicyOverrideAdjuster(8197.0),
                new CountryRollingPolicyOverrideAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.34, ["central"] = 1.1, ["north"] = 1.15 }),
                new FlaggedRollingPolicyOverrideAdjuster(flags, "rolling-policy-override-strict", -1.0),
            },
            new List<string>(),
            4.5,
            10470.3);
    }

    public double Run(RollingPolicyOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingPolicyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
