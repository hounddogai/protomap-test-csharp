namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled access badge target calculation.</summary>
public interface IScheduledAccessBadgeTargetStep
{
    string Describe();
    double Adjust(ScheduledAccessBadgeTarget item, double valueValue);
}

public sealed record FlaggedScheduledAccessBadgeTargetAdjuster(Registry Flags, string Flag, double Percent) : IScheduledAccessBadgeTargetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledAccessBadgeTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameScheduledAccessBadgeTargetAdjuster(Dictionary<string, double> Factors) : IScheduledAccessBadgeTargetStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(ScheduledAccessBadgeTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledAccessBadgeTargetAdjuster(double Cap) : IScheduledAccessBadgeTargetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledAccessBadgeTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentScheduledAccessBadgeTargetAdjuster(double Percent) : IScheduledAccessBadgeTargetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledAccessBadgeTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledAccessBadgeTargetAdjuster(double Minimum) : IScheduledAccessBadgeTargetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledAccessBadgeTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledAccessBadgeTargetAdjuster(List<double> Bounds, List<double> Rates) : IScheduledAccessBadgeTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledAccessBadgeTarget item, double valueValue)
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

/// <summary>Runs the scheduled access badge target steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAccessBadgeTargetEngine(List<IScheduledAccessBadgeTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAccessBadgeTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAccessBadgeTargetEngine DefaultScheduledAccessBadgeTargetEngine(Registry flags)
    {
        return new ScheduledAccessBadgeTargetEngine(
            new List<IScheduledAccessBadgeTargetStep>
            {
                new FlaggedScheduledAccessBadgeTargetAdjuster(flags, "scheduled-access-badge-target-default", 0.8),
                new NameScheduledAccessBadgeTargetAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.2, ["retail"] = 1.16 }),
                new CapScheduledAccessBadgeTargetAdjuster(9953.0),
                new PercentScheduledAccessBadgeTargetAdjuster(12.3),
                new MinimumScheduledAccessBadgeTargetAdjuster(14.3),
                new TieredScheduledAccessBadgeTargetAdjuster(
                    new List<double> { 2457.0, 5505.0, 7176.0 },
                    new List<double> { 0.99, 1.13, 1.12 }),
            },
            new List<string>(),
            3.3,
            52546.2);
    }

    public double Compute(ScheduledAccessBadgeTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAccessBadgeTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
