namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request notice calculation.</summary>
public interface ILeaveRequestNoticeStep
{
    string Describe();
    double Adjust(LeaveRequestNotice item, double valueValue);
}

public sealed record CapLeaveRequestNoticeAdjuster(double Cap) : ILeaveRequestNoticeStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLeaveRequestNoticeAdjuster(double Minimum) : ILeaveRequestNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLeaveRequestNoticeAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestNotice item, double valueValue)
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

public sealed record LeaveRequestNoticeAttemptsOffsetAdjuster(double Weight) : ILeaveRequestNoticeStep
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(LeaveRequestNotice item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record NameLeaveRequestNoticeAdjuster(Dictionary<string, double> Factors) : ILeaveRequestNoticeStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LeaveRequestNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLeaveRequestNoticeAdjuster(double Step) : ILeaveRequestNoticeStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the leave request notice steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestNoticeEngine(List<ILeaveRequestNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestNoticeEngine DefaultLeaveRequestNoticeEngine()
    {
        return new LeaveRequestNoticeEngine(
            new List<ILeaveRequestNoticeStep>
            {
                new CapLeaveRequestNoticeAdjuster(13213.0),
                new MinimumLeaveRequestNoticeAdjuster(4.0),
                new TieredLeaveRequestNoticeAdjuster(
                    new List<double> { 1627.0, 1892.0, 5821.0 },
                    new List<double> { 1.18, 0.93, 1.07 }),
                new LeaveRequestNoticeAttemptsOffsetAdjuster(0.44),
                new NameLeaveRequestNoticeAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.25, ["retail"] = 0.8 }),
                new RoundingLeaveRequestNoticeAdjuster(5.0),
            },
            new List<string>(),
            4.7,
            89898.3);
    }

    public double Run(LeaveRequestNotice item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
