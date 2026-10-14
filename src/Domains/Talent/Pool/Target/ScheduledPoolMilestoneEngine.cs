namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled pool milestone calculation.</summary>
public interface IScheduledPoolMilestoneModifier
{
    string Describe();
    double Adjust(ScheduledPoolMilestone item, double valueValue);
}

public sealed record MinimumScheduledPoolMilestoneAdjuster(double Minimum) : IScheduledPoolMilestoneModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledPoolMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedScheduledPoolMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IScheduledPoolMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledPoolMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledPoolMilestoneQuantityOffsetAdjuster(double Weight) : IScheduledPoolMilestoneModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(ScheduledPoolMilestone item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record TieredScheduledPoolMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IScheduledPoolMilestoneModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledPoolMilestone item, double valueValue)
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

public sealed record PercentScheduledPoolMilestoneAdjuster(double Percent) : IScheduledPoolMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPoolMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledPoolMilestoneAdjuster(double Step) : IScheduledPoolMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledPoolMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled pool milestone steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPoolMilestoneEngine(List<IScheduledPoolMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPoolMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPoolMilestoneEngine DefaultScheduledPoolMilestoneEngine(Registry flags)
    {
        return new ScheduledPoolMilestoneEngine(
            new List<IScheduledPoolMilestoneModifier>
            {
                new MinimumScheduledPoolMilestoneAdjuster(8.6),
                new FlaggedScheduledPoolMilestoneAdjuster(flags, "scheduled-pool-milestone-adaptive", 5.7),
                new ScheduledPoolMilestoneQuantityOffsetAdjuster(0.68),
                new TieredScheduledPoolMilestoneAdjuster(
                    new List<double> { 27.0, 828.0, 5876.0, 7337.0 },
                    new List<double> { 0.91, 0.9, 1.1, 1.19 }),
                new PercentScheduledPoolMilestoneAdjuster(-21.3),
                new RoundingScheduledPoolMilestoneAdjuster(0.5),
            },
            new List<string>(),
            4.3,
            12060.2);
    }

    public double Evaluate(ScheduledPoolMilestone item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPoolMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
