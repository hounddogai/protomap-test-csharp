namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled leave request override calculation.</summary>
public interface IScheduledLeaveRequestOverrideFactor
{
    string Describe();
    double Adjust(ScheduledLeaveRequestOverride item, double valueValue);
}

public sealed record FlaggedScheduledLeaveRequestOverrideAdjuster(Registry Flags, string Flag, double Percent) : IScheduledLeaveRequestOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledLeaveRequestOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledLeaveRequestOverrideAdjuster(double Minimum) : IScheduledLeaveRequestOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledLeaveRequestOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScheduledLeaveRequestOverrideAdjuster(double Cap) : IScheduledLeaveRequestOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledLeaveRequestOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled leave request override steps in order and keeps a trace of each.</summary>
public sealed class ScheduledLeaveRequestOverrideEngine(List<IScheduledLeaveRequestOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledLeaveRequestOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledLeaveRequestOverrideEngine DefaultScheduledLeaveRequestOverrideEngine(Registry flags)
    {
        return new ScheduledLeaveRequestOverrideEngine(
            new List<IScheduledLeaveRequestOverrideFactor>
            {
                new FlaggedScheduledLeaveRequestOverrideAdjuster(
                    flags,
                    "scheduled-leave-request-override-manual",
                    3.6),
                new MinimumScheduledLeaveRequestOverrideAdjuster(43.3),
                new CapScheduledLeaveRequestOverrideAdjuster(11677.0),
            },
            new List<string>(),
            0.4,
            9487.7);
    }

    public double Price(ScheduledLeaveRequestOverride item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledLeaveRequestOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
