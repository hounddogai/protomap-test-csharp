namespace Acme.Workforce.Absence.LeaveRequest.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request override calculation.</summary>
public interface ILeaveRequestOverrideStep
{
    string Describe();
    double Adjust(LeaveRequestOverride item, double valueValue);
}

public sealed record FlaggedLeaveRequestOverrideAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelLeaveRequestOverrideAdjuster(Dictionary<string, double> Factors) : ILeaveRequestOverrideStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LeaveRequestOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LeaveRequestOverrideGraceDaysOffsetAdjuster(double Weight) : ILeaveRequestOverrideStep
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(LeaveRequestOverride item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record PercentLeaveRequestOverrideAdjuster(double Percent) : ILeaveRequestOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request override steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestOverrideEngine(List<ILeaveRequestOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestOverrideEngine DefaultLeaveRequestOverrideEngine(Registry flags)
    {
        return new LeaveRequestOverrideEngine(
            new List<ILeaveRequestOverrideStep>
            {
                new FlaggedLeaveRequestOverrideAdjuster(flags, "leave-request-override-fallback", 9.3),
                new ChannelLeaveRequestOverrideAdjuster(new Dictionary<string, double> { ["retail"] = 1.34 }),
                new LeaveRequestOverrideGraceDaysOffsetAdjuster(0.51),
                new PercentLeaveRequestOverrideAdjuster(-9.6),
            },
            new List<string>(),
            3.7,
            4296.2);
    }

    public double Price(LeaveRequestOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
