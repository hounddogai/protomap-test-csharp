namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending holiday reservation is in its life cycle.</summary>
public enum PendingHolidayReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a pending holiday reservation and its current status.</summary>
public sealed class PendingHolidayReservation(string code, string title, bool approved, string employeeId, string department, double rate, int createdAt, string notes, int expiresAt, PendingHolidayReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string EmployeeId { get; set; } = employeeId;
    public string Department { get; set; } = department;
    public double Rate { get; set; } = rate;
    public int CreatedAt { get; set; } = createdAt;
    public string Notes { get; set; } = notes;
    public int ExpiresAt { get; set; } = expiresAt;
    public PendingHolidayReservationStatus Status { get; set; } = status;

    /// <summary>Starts a pending holiday reservation in status proposed.</summary>
    public static PendingHolidayReservation NewPendingHolidayReservation(string code, string title)
    {
        return new PendingHolidayReservation(
            code,
            title,
            false,
            "",
            "",
            0.0,
            0,
            "",
            0,
            PendingHolidayReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingHolidayReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingHolidayReservationStatus next)
    {
        switch (Status)
        {
            case PendingHolidayReservationStatus.Proposed:
                switch (next)
                {
                    case PendingHolidayReservationStatus.Approved:
                    case PendingHolidayReservationStatus.Rejected:
                        return true;
                }
                return false;
            case PendingHolidayReservationStatus.Approved:
                return next == PendingHolidayReservationStatus.Rejected || next == PendingHolidayReservationStatus.Withdrawn;
            case PendingHolidayReservationStatus.Rejected:
                return next == PendingHolidayReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PendingHolidayReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending holiday reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending holiday reservation"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Rate * (double)CreatedAt, 2);
    }

    public PendingHolidayReservation Copy()
    {
        return new PendingHolidayReservation(
            Code,
            Title,
            Approved,
            EmployeeId,
            Department,
            Rate,
            CreatedAt,
            Notes,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
