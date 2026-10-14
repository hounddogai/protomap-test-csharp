namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default pool milestone calculation.</summary>
public interface IDefaultPoolMilestoneModifier
{
    string Describe();
    double Adjust(DefaultPoolMilestone item, double valueValue);
}

public sealed record MinimumDefaultPoolMilestoneAdjuster(double Minimum) : IDefaultPoolMilestoneModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPoolMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultPoolMilestoneAdjuster(double Percent) : IDefaultPoolMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPoolMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedDefaultPoolMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IDefaultPoolMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultPoolMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default pool milestone steps in order and keeps a trace of each.</summary>
public sealed class DefaultPoolMilestoneEngine(List<IDefaultPoolMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPoolMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPoolMilestoneEngine DefaultDefaultPoolMilestoneEngine(Registry flags)
    {
        return new DefaultPoolMilestoneEngine(
            new List<IDefaultPoolMilestoneModifier>
            {
                new MinimumDefaultPoolMilestoneAdjuster(18.6),
                new PercentDefaultPoolMilestoneAdjuster(-11.1),
                new FlaggedDefaultPoolMilestoneAdjuster(flags, "default-pool-milestone-default", 8.2),
            },
            new List<string>(),
            3.2,
            2362.8);
    }

    public double Run(DefaultPoolMilestone item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPoolMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
