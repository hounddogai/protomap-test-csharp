namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy bundle calculation.</summary>
public interface IPolicyBundleStep
{
    string Describe();
    double Adjust(PolicyBundle item, double valueValue);
}

public sealed record FlaggedPolicyBundleAdjuster(Registry Flags, string Flag, double Percent) : IPolicyBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitPolicyBundleAdjuster(Dictionary<string, double> Factors) : IPolicyBundleStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PolicyBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PolicyBundleFloorValueOffsetAdjuster(double Weight) : IPolicyBundleStep
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(PolicyBundle item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record MinimumPolicyBundleAdjuster(double Minimum) : IPolicyBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPolicyBundleAdjuster(double Step) : IPolicyBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPolicyBundleAdjuster(double Percent) : IPolicyBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy bundle steps in order and keeps a trace of each.</summary>
public sealed class PolicyBundleEngine(List<IPolicyBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyBundleEngine DefaultPolicyBundleEngine(Registry flags)
    {
        return new PolicyBundleEngine(
            new List<IPolicyBundleStep>
            {
                new FlaggedPolicyBundleAdjuster(flags, "policy-bundle-adaptive", 3.3),
                new UnitPolicyBundleAdjuster(new Dictionary<string, double> { ["east"] = 0.72 }),
                new PolicyBundleFloorValueOffsetAdjuster(0.52),
                new MinimumPolicyBundleAdjuster(3.5),
                new RoundingPolicyBundleAdjuster(5.0),
                new PercentPolicyBundleAdjuster(4.9),
            },
            new List<string>(),
            2.0,
            94436.8);
    }

    public double Run(PolicyBundle item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
