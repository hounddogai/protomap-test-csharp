namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient engagement policy calculation.</summary>
public interface ILenientEngagementPolicyStep
{
    string Describe();
    double Adjust(LenientEngagementPolicy item, double valueValue);
}

public sealed record FlaggedLenientEngagementPolicyAdjuster(Registry Flags, string Flag, double Percent) : ILenientEngagementPolicyStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientEngagementPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientEngagementPolicyAdjuster(double Cap) : ILenientEngagementPolicyStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientEngagementPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientEngagementPolicyAdjuster(double Percent) : ILenientEngagementPolicyStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientEngagementPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient engagement policy steps in order and keeps a trace of each.</summary>
public sealed class LenientEngagementPolicyEngine(List<ILenientEngagementPolicyStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientEngagementPolicyStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientEngagementPolicyEngine DefaultLenientEngagementPolicyEngine(Registry flags)
    {
        return new LenientEngagementPolicyEngine(
            new List<ILenientEngagementPolicyStep>
            {
                new FlaggedLenientEngagementPolicyAdjuster(flags, "lenient-engagement-policy-default", 5.3),
                new CapLenientEngagementPolicyAdjuster(6647.0),
                new PercentLenientEngagementPolicyAdjuster(7.0),
            },
            new List<string>(),
            3.6,
            35290.9);
    }

    public double Evaluate(LenientEngagementPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientEngagementPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
