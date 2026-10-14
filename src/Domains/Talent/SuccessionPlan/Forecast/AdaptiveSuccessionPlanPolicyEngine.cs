namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive succession plan policy calculation.</summary>
public interface IAdaptiveSuccessionPlanPolicyFactor
{
    string Describe();
    double Adjust(AdaptiveSuccessionPlanPolicy item, double valueValue);
}

public sealed record CapAdaptiveSuccessionPlanPolicyAdjuster(double Cap) : IAdaptiveSuccessionPlanPolicyFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveSuccessionPlanPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAdaptiveSuccessionPlanPolicyAdjuster(double Percent) : IAdaptiveSuccessionPlanPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveSuccessionPlanPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveSuccessionPlanPolicyAdjuster(double Minimum) : IAdaptiveSuccessionPlanPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveSuccessionPlanPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAdaptiveSuccessionPlanPolicyAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveSuccessionPlanPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveSuccessionPlanPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AdaptiveSuccessionPlanPolicyLimitOffsetAdjuster(double Weight) : IAdaptiveSuccessionPlanPolicyFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(AdaptiveSuccessionPlanPolicy item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record NameAdaptiveSuccessionPlanPolicyAdjuster(Dictionary<string, double> Factors) : IAdaptiveSuccessionPlanPolicyFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveSuccessionPlanPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive succession plan policy steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveSuccessionPlanPolicyEngine(List<IAdaptiveSuccessionPlanPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveSuccessionPlanPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveSuccessionPlanPolicyEngine DefaultAdaptiveSuccessionPlanPolicyEngine(Registry flags)
    {
        return new AdaptiveSuccessionPlanPolicyEngine(
            new List<IAdaptiveSuccessionPlanPolicyFactor>
            {
                new CapAdaptiveSuccessionPlanPolicyAdjuster(37571.0),
                new PercentAdaptiveSuccessionPlanPolicyAdjuster(-9.7),
                new MinimumAdaptiveSuccessionPlanPolicyAdjuster(44.5),
                new FlaggedAdaptiveSuccessionPlanPolicyAdjuster(
                    flags,
                    "adaptive-succession-plan-policy-capped",
                    -1.3),
                new AdaptiveSuccessionPlanPolicyLimitOffsetAdjuster(0.89),
                new NameAdaptiveSuccessionPlanPolicyAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.34, ["retail"] = 1.35, ["east"] = 1.05 }),
            },
            new List<string>(),
            3.7,
            75880.5);
    }

    public double Run(AdaptiveSuccessionPlanPolicy item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveSuccessionPlanPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
