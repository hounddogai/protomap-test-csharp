namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the leave request slot calculation.</summary>
public interface ILeaveRequestSlotModifier
{
    string Describe();
    double Adjust(LeaveRequestSlot item, double valueValue);
}

public sealed record LeaveRequestSlotAttemptsOffsetAdjuster(double Weight) : ILeaveRequestSlotModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(LeaveRequestSlot item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record RoundingLeaveRequestSlotAdjuster(double Step) : ILeaveRequestSlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LeaveRequestSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLeaveRequestSlotAdjuster(List<double> Bounds, List<double> Rates) : ILeaveRequestSlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LeaveRequestSlot item, double valueValue)
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

public sealed record CapLeaveRequestSlotAdjuster(double Cap) : ILeaveRequestSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LeaveRequestSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLeaveRequestSlotAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLeaveRequestSlotAdjuster(double Percent) : ILeaveRequestSlotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the leave request slot steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestSlotEngine(List<ILeaveRequestSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestSlotEngine DefaultLeaveRequestSlotEngine(Registry flags)
    {
        return new LeaveRequestSlotEngine(
            new List<ILeaveRequestSlotModifier>
            {
                new LeaveRequestSlotAttemptsOffsetAdjuster(1.05),
                new RoundingLeaveRequestSlotAdjuster(1.0),
                new TieredLeaveRequestSlotAdjuster(
                    new List<double> { 3148.0, 6271.0 },
                    new List<double> { 0.99, 1.18 }),
                new CapLeaveRequestSlotAdjuster(27718.0),
                new FlaggedLeaveRequestSlotAdjuster(flags, "leave-request-slot-scheduled", -4.8),
                new PercentLeaveRequestSlotAdjuster(12.2),
            },
            new List<string>(),
            0.8,
            23293.6);
    }

    public double Evaluate(LeaveRequestSlot item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
