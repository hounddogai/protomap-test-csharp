namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the leave request reservation calculation.</summary>
public interface ILeaveRequestReservationModifier
{
    string Describe();
    double Adjust(LeaveRequestReservation item, double valueValue);
}

public sealed record FlaggedLeaveRequestReservationAdjuster(Registry Flags, string Flag, double Percent) : ILeaveRequestReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LeaveRequestReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLeaveRequestReservationAdjuster(double Percent) : ILeaveRequestReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LeaveRequestReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LeaveRequestReservationHoursOffsetAdjuster(double Weight) : ILeaveRequestReservationModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LeaveRequestReservation item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the leave request reservation steps in order and keeps a trace of each.</summary>
public sealed class LeaveRequestReservationEngine(List<ILeaveRequestReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILeaveRequestReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LeaveRequestReservationEngine DefaultLeaveRequestReservationEngine(Registry flags)
    {
        return new LeaveRequestReservationEngine(
            new List<ILeaveRequestReservationModifier>
            {
                new FlaggedLeaveRequestReservationAdjuster(flags, "leave-request-reservation-manual", -3.0),
                new PercentLeaveRequestReservationAdjuster(17.0),
                new LeaveRequestReservationHoursOffsetAdjuster(0.39),
            },
            new List<string>(),
            0.2,
            5398.0);
    }

    public double Compute(LeaveRequestReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<LeaveRequestReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
