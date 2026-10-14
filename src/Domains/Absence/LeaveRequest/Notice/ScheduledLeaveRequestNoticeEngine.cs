namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled leave request notice calculation.</summary>
public interface IScheduledLeaveRequestNoticeFactor
{
    string Describe();
    double Adjust(ScheduledLeaveRequestNotice item, double valueValue);
}

public sealed record ChannelScheduledLeaveRequestNoticeAdjuster(Dictionary<string, double> Factors) : IScheduledLeaveRequestNoticeFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ScheduledLeaveRequestNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScheduledLeaveRequestNoticeAdjuster(List<double> Bounds, List<double> Rates) : IScheduledLeaveRequestNoticeFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledLeaveRequestNotice item, double valueValue)
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

public sealed record MinimumScheduledLeaveRequestNoticeAdjuster(double Minimum) : IScheduledLeaveRequestNoticeFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledLeaveRequestNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedScheduledLeaveRequestNoticeAdjuster(Registry Flags, string Flag, double Percent) : IScheduledLeaveRequestNoticeFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledLeaveRequestNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledLeaveRequestNoticeAdjuster(double Step) : IScheduledLeaveRequestNoticeFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledLeaveRequestNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled leave request notice steps in order and keeps a trace of each.</summary>
public sealed class ScheduledLeaveRequestNoticeEngine(List<IScheduledLeaveRequestNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledLeaveRequestNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledLeaveRequestNoticeEngine DefaultScheduledLeaveRequestNoticeEngine(Registry flags)
    {
        return new ScheduledLeaveRequestNoticeEngine(
            new List<IScheduledLeaveRequestNoticeFactor>
            {
                new ChannelScheduledLeaveRequestNoticeAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.8, ["west"] = 1.03 }),
                new TieredScheduledLeaveRequestNoticeAdjuster(
                    new List<double> { 7210.0, 9070.0 },
                    new List<double> { 1.04, 1.0 }),
                new MinimumScheduledLeaveRequestNoticeAdjuster(24.8),
                new FlaggedScheduledLeaveRequestNoticeAdjuster(flags, "scheduled-leave-request-notice-lenient", -3.2),
                new RoundingScheduledLeaveRequestNoticeAdjuster(5.0),
            },
            new List<string>(),
            1.6,
            73975.3);
    }

    public double Compute(ScheduledLeaveRequestNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledLeaveRequestNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
