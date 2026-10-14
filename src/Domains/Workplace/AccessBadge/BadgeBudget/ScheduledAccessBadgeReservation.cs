namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled access badge reservation is in its life cycle.</summary>
public enum ScheduledAccessBadgeReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled access badge reservation. Its code is unique within a tenant.</summary>
public sealed class ScheduledAccessBadgeReservation(string code, string title, string employeeId, double hours, double discount, string region, double rate, double minimum, int windowDays, ScheduledAccessBadgeReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public double Hours { get; set; } = hours;
    public double Discount { get; set; } = discount;
    public string Region { get; set; } = region;
    public double Rate { get; set; } = rate;
    public double Minimum { get; set; } = minimum;
    public int WindowDays { get; set; } = windowDays;
    public ScheduledAccessBadgeReservationStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled access badge reservation in status proposed.</summary>
    public static ScheduledAccessBadgeReservation NewScheduledAccessBadgeReservation(string code, string title)
    {
        return new ScheduledAccessBadgeReservation(
            code,
            title,
            "",
            0.0,
            0.0,
            "",
            0.0,
            0.0,
            0,
            ScheduledAccessBadgeReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAccessBadgeReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAccessBadgeReservationStatus next)
    {
        switch (Status)
        {
            case ScheduledAccessBadgeReservationStatus.Proposed:
                return next == ScheduledAccessBadgeReservationStatus.Rejected;
            case ScheduledAccessBadgeReservationStatus.Approved:
                return next == ScheduledAccessBadgeReservationStatus.Rejected || next == ScheduledAccessBadgeReservationStatus.Withdrawn;
            case ScheduledAccessBadgeReservationStatus.Rejected:
                switch (next)
                {
                    case ScheduledAccessBadgeReservationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledAccessBadgeReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled access badge reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled access badge reservation"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Discount - Rate, 2);
    }

    public ScheduledAccessBadgeReservation Copy()
    {
        return new ScheduledAccessBadgeReservation(
            Code,
            Title,
            EmployeeId,
            Hours,
            Discount,
            Region,
            Rate,
            Minimum,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
