namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request draft calculation.</summary>
public interface ILeaveRequestDraftStep
{
    string Describe();
    double Adjust(LeaveRequestDraft item, double valueValue);
}

public sealed record RoundingLeaveRequestDraftAdjuster(double Step) : ILeaveRequestDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLeaveRequestDraftAdjuster(double Percent) : ILeaveRequestDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelLeaveRequestDraftAdjuster(Dictionary<string, double> Factors) : ILeaveRequestDraftStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LeaveRequestDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LeaveRequestDraftRateOffsetAdjuster(double Weight) : ILeaveRequestDraftStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(LeaveRequestDraft item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record FlaggedLeaveRequestDraftAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request draft steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestDraftEngine(List<ILeaveRequestDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestDraftEngine DefaultLeaveRequestDraftEngine(Registry flags)
    {
        return new LeaveRequestDraftEngine(
            new List<ILeaveRequestDraftStep>
            {
                new RoundingLeaveRequestDraftAdjuster(0.1),
                new PercentLeaveRequestDraftAdjuster(3.7),
                new ChannelLeaveRequestDraftAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.33, ["south"] = 0.72 }),
                new LeaveRequestDraftRateOffsetAdjuster(1.75),
                new FlaggedLeaveRequestDraftAdjuster(flags, "leave-request-draft-strict", 4.7),
            },
            new List<string>(),
            1.2,
            26474.0);
    }

    public double Compute(LeaveRequestDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
