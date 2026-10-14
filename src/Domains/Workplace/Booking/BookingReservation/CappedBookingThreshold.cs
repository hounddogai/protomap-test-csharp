namespace Acme.Workforce.Workplace.Booking.BookingReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped booking threshold is in its life cycle.</summary>
public enum CappedBookingThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One capped booking threshold, with what the planners need to act on it.</summary>
public sealed class CappedBookingThreshold(string code, string name, double cost, string location, string skill, int sequence, string title, int attempts, int priority, CappedBookingThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Location { get; set; } = location;
    public string Skill { get; set; } = skill;
    public int Sequence { get; set; } = sequence;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public CappedBookingThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a capped booking threshold in status new.</summary>
    public static CappedBookingThreshold NewCappedBookingThreshold(string code, string name)
    {
        return new CappedBookingThreshold(code, name, 0.0, "", "", 0, "", 0, 0, CappedBookingThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedBookingThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedBookingThresholdStatus next)
    {
        switch (Status)
        {
            case CappedBookingThresholdStatus.New:
                return next == CappedBookingThresholdStatus.Queued || next == CappedBookingThresholdStatus.Running;
            case CappedBookingThresholdStatus.Queued:
                return next == CappedBookingThresholdStatus.Failed;
            case CappedBookingThresholdStatus.Running:
                return next == CappedBookingThresholdStatus.Failed;
            case CappedBookingThresholdStatus.Done:
                return next == CappedBookingThresholdStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedBookingThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped booking threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped booking threshold"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)Attempts + (double)Priority, 2);
    }

    public CappedBookingThreshold Copy()
    {
        return new CappedBookingThreshold(
            Code,
            Name,
            Cost,
            Location,
            Skill,
            Sequence,
            Title,
            Attempts,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
