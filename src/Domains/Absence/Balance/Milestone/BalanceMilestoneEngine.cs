namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance milestone calculation.</summary>
public interface IBalanceMilestoneModifier
{
    string Describe();
    double Adjust(BalanceMilestone item, double valueValue);
}

public sealed record RoundingBalanceMilestoneAdjuster(double Step) : IBalanceMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBalanceMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IBalanceMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBalanceMilestoneAdjuster(double Minimum) : IBalanceMilestoneModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBalanceMilestoneAdjuster(double Percent) : IBalanceMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelBalanceMilestoneAdjuster(Dictionary<string, double> Factors) : IBalanceMilestoneModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BalanceMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the balance milestone steps in order and keeps a trace of each.</summary>
public sealed class BalanceMilestoneEngine(List<IBalanceMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceMilestoneEngine DefaultBalanceMilestoneEngine(Registry flags)
    {
        return new BalanceMilestoneEngine(
            new List<IBalanceMilestoneModifier>
            {
                new RoundingBalanceMilestoneAdjuster(0.5),
                new FlaggedBalanceMilestoneAdjuster(flags, "balance-milestone-fallback", 6.0),
                new MinimumBalanceMilestoneAdjuster(11.5),
                new PercentBalanceMilestoneAdjuster(13.3),
                new LabelBalanceMilestoneAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.86, ["west"] = 1.39, ["south"] = 1.32 }),
            },
            new List<string>(),
            4.9,
            93446.4);
    }

    public double Run(BalanceMilestone item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
