namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling access badge tier calculation.</summary>
public interface IRollingAccessBadgeTierFactor
{
    string Describe();
    double Adjust(RollingAccessBadgeTier item, double valueValue);
}

public sealed record RoundingRollingAccessBadgeTierAdjuster(double Step) : IRollingAccessBadgeTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingAccessBadgeTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRollingAccessBadgeTierAdjuster(double Percent) : IRollingAccessBadgeTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingAccessBadgeTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRollingAccessBadgeTierAdjuster(Registry Flags, string Flag, double Percent) : IRollingAccessBadgeTierFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingAccessBadgeTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling access badge tier steps in order and keeps a trace of each.</summary>
public sealed class RollingAccessBadgeTierEngine(List<IRollingAccessBadgeTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAccessBadgeTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAccessBadgeTierEngine DefaultRollingAccessBadgeTierEngine(Registry flags)
    {
        return new RollingAccessBadgeTierEngine(
            new List<IRollingAccessBadgeTierFactor>
            {
                new RoundingRollingAccessBadgeTierAdjuster(0.05),
                new PercentRollingAccessBadgeTierAdjuster(-8.9),
                new FlaggedRollingAccessBadgeTierAdjuster(flags, "rolling-access-badge-tier-weighted", 2.7),
            },
            new List<string>(),
            2.8,
            31176.7);
    }

    public double Run(RollingAccessBadgeTier item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAccessBadgeTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
