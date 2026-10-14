namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request reservation is in its life cycle.</summary>
public enum LeaveRequestReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One leave request reservation, with what the planners need to act on it.</summary>
public sealed class LeaveRequestReservation(string code, string name, string contactEmail, double hours, bool approved, LeaveRequestReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public double Hours { get; set; } = hours;
    public bool Approved { get; set; } = approved;
    public LeaveRequestReservationStatus Status { get; set; } = status;

    /// <summary>Starts a leave request reservation in status requested.</summary>
    public static LeaveRequestReservation NewLeaveRequestReservation(string code, string name)
    {
        return new LeaveRequestReservation(code, name, "", 0.0, false, LeaveRequestReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestReservationStatus next)
    {
        switch (Status)
        {
            case LeaveRequestReservationStatus.Requested:
                return next == LeaveRequestReservationStatus.Expired;
            case LeaveRequestReservationStatus.Granted:
                return next == LeaveRequestReservationStatus.Expired;
            case LeaveRequestReservationStatus.Revoked:
                return next == LeaveRequestReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LeaveRequestReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public LeaveRequestReservation Copy()
    {
        return new LeaveRequestReservation(Code, Name, ContactEmail, Hours, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
