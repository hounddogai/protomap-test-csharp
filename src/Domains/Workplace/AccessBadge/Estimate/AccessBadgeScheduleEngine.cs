namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the access badge schedule calculation.</summary>
public interface IAccessBadgeScheduleAdjuster
{
    string Describe();
    double Adjust(AccessBadgeSchedule item, double valueValue);
}

public sealed record MinimumAccessBadgeScheduleAdjuster(double Minimum) : IAccessBadgeScheduleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeSchedule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAccessBadgeScheduleAdjuster(double Percent) : IAccessBadgeScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAccessBadgeScheduleAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeScheduleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record UnitAccessBadgeScheduleAdjuster(Dictionary<string, double> Factors) : IAccessBadgeScheduleAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(AccessBadgeSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the access badge schedule steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeScheduleEngine(List<IAccessBadgeScheduleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeScheduleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeScheduleEngine DefaultAccessBadgeScheduleEngine(Registry flags)
    {
        return new AccessBadgeScheduleEngine(
            new List<IAccessBadgeScheduleAdjuster>
            {
                new MinimumAccessBadgeScheduleAdjuster(1.5),
                new PercentAccessBadgeScheduleAdjuster(-24.6),
                new FlaggedAccessBadgeScheduleAdjuster(flags, "access-badge-schedule-batched", 5.0),
                new UnitAccessBadgeScheduleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.88, ["central"] = 0.74 }),
            },
            new List<string>(),
            3.0,
            3897.8);
    }

    public double Evaluate(AccessBadgeSchedule item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
