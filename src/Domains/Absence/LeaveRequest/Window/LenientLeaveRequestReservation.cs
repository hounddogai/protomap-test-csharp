namespace Acme.Workforce.Absence.LeaveRequest.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient leave request reservation is in its life cycle.</summary>
public enum LenientLeaveRequestReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient leave request reservation, with what the planners need to act on it.</summary>
public sealed class LenientLeaveRequestReservation(string code, string title, bool active, int tenureMonths, double budget, string costCenter, LenientLeaveRequestReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Budget { get; set; } = budget;
    public string CostCenter { get; set; } = costCenter;
    public LenientLeaveRequestReservationStatus Status { get; set; } = status;

    /// <summary>Starts a lenient leave request reservation in status requested.</summary>
    public static LenientLeaveRequestReservation NewLenientLeaveRequestReservation(string code, string title)
    {
        return new LenientLeaveRequestReservation(
            code,
            title,
            false,
            0,
            0.0,
            "",
            LenientLeaveRequestReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLeaveRequestReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLeaveRequestReservationStatus next)
    {
        switch (Status)
        {
            case LenientLeaveRequestReservationStatus.Requested:
                return next == LenientLeaveRequestReservationStatus.Granted || next == LenientLeaveRequestReservationStatus.Expired;
            case LenientLeaveRequestReservationStatus.Granted:
                switch (next)
                {
                    case LenientLeaveRequestReservationStatus.Revoked:
                    case LenientLeaveRequestReservationStatus.Expired:
                        return true;
                }
                return false;
            case LenientLeaveRequestReservationStatus.Revoked:
                return next == LenientLeaveRequestReservationStatus.Expired;
        }
        return false;
    }

    public void MoveTo(LenientLeaveRequestReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient leave request reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Budget + (double)TenureMonths, 2);
    }

    public LenientLeaveRequestReservation Copy()
    {
        return new LenientLeaveRequestReservation(Code, Title, Active, TenureMonths, Budget, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
