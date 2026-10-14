namespace Acme.Workforce.Absence.LeaveRequest.Reservation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the manual leave request reservation calculation.</summary>
public interface IManualLeaveRequestReservationStep
{
    string Describe();
    double Adjust(ManualLeaveRequestReservation item, double valueValue);
}

public sealed record PercentManualLeaveRequestReservationAdjuster(double Percent) : IManualLeaveRequestReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualLeaveRequestReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualLeaveRequestReservationAdjuster(double Cap) : IManualLeaveRequestReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualLeaveRequestReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualLeaveRequestReservationMaximumOffsetAdjuster(double Weight) : IManualLeaveRequestReservationStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(ManualLeaveRequestReservation item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record FlaggedManualLeaveRequestReservationAdjuster(Registry Flags, string Flag, double Percent) : IManualLeaveRequestReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualLeaveRequestReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual leave request reservation steps in order and keeps a trace of each.</summary>
public sealed class ManualLeaveRequestReservationEngine(List<IManualLeaveRequestReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualLeaveRequestReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualLeaveRequestReservationEngine DefaultManualLeaveRequestReservationEngine(Registry flags)
    {
        return new ManualLeaveRequestReservationEngine(
            new List<IManualLeaveRequestReservationStep>
            {
                new PercentManualLeaveRequestReservationAdjuster(-18.8),
                new CapManualLeaveRequestReservationAdjuster(7155.0),
                new ManualLeaveRequestReservationMaximumOffsetAdjuster(1.89),
                new FlaggedManualLeaveRequestReservationAdjuster(
                    flags,
                    "manual-leave-request-reservation-tiered",
                    8.1),
            },
            new List<string>(),
            3.9,
            26708.6);
    }

    public double Compute(ManualLeaveRequestReservation item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ManualLeaveRequestReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
