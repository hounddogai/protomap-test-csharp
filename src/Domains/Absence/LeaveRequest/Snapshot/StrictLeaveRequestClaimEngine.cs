namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict leave request claim calculation.</summary>
public interface IStrictLeaveRequestClaimAdjuster
{
    string Describe();
    double Adjust(StrictLeaveRequestClaim item, double valueValue);
}

public sealed record FlaggedStrictLeaveRequestClaimAdjuster(Registry Flags, string Flag, double Percent) : IStrictLeaveRequestClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictLeaveRequestClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StrictLeaveRequestClaimWindowDaysOffsetAdjuster(double Weight) : IStrictLeaveRequestClaimAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(StrictLeaveRequestClaim item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record LabelStrictLeaveRequestClaimAdjuster(Dictionary<string, double> Factors) : IStrictLeaveRequestClaimAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictLeaveRequestClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict leave request claim steps in order and keeps a trace of each.</summary>
public sealed class StrictLeaveRequestClaimEngine(List<IStrictLeaveRequestClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictLeaveRequestClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictLeaveRequestClaimEngine DefaultStrictLeaveRequestClaimEngine(Registry flags)
    {
        return new StrictLeaveRequestClaimEngine(
            new List<IStrictLeaveRequestClaimAdjuster>
            {
                new FlaggedStrictLeaveRequestClaimAdjuster(flags, "strict-leave-request-claim-fallback", 7.7),
                new StrictLeaveRequestClaimWindowDaysOffsetAdjuster(0.32),
                new LabelStrictLeaveRequestClaimAdjuster(new Dictionary<string, double> { ["retail"] = 0.73 }),
            },
            new List<string>(),
            0.7,
            78852.3);
    }

    public double Run(StrictLeaveRequestClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictLeaveRequestClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
