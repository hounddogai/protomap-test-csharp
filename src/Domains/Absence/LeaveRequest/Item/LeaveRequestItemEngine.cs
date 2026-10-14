namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request item calculation.</summary>
public interface ILeaveRequestItemModifier
{
    string Describe();
    double Adjust(LeaveRequestItem item, double valueValue);
}

public sealed record MinimumLeaveRequestItemAdjuster(double Minimum) : ILeaveRequestItemModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LeaveRequestItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLeaveRequestItemAdjuster(double Cap) : ILeaveRequestItemModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLeaveRequestItemAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestItemModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLeaveRequestItemAdjuster(double Step) : ILeaveRequestItemModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestItem item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the leave request item steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestItemEngine(List<ILeaveRequestItemModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestItemModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestItemEngine DefaultLeaveRequestItemEngine(Registry flags)
    {
        return new LeaveRequestItemEngine(
            new List<ILeaveRequestItemModifier>
            {
                new MinimumLeaveRequestItemAdjuster(41.5),
                new CapLeaveRequestItemAdjuster(42369.0),
                new FlaggedLeaveRequestItemAdjuster(flags, "leave-request-item-regional", 3.3),
                new RoundingLeaveRequestItemAdjuster(5.0),
            },
            new List<string>(),
            3.8,
            70903.3);
    }

    public double Compute(LeaveRequestItem item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
