namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled nomination reservation is in its life cycle.</summary>
public enum ScheduledNominationReservationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled nomination reservation. Its code is unique within a tenant.</summary>
public sealed class ScheduledNominationReservation(string code, string title, double fte, int windowDays, string category, double margin, ScheduledNominationReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public int WindowDays { get; set; } = windowDays;
    public string Category { get; set; } = category;
    public double Margin { get; set; } = margin;
    public ScheduledNominationReservationStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled nomination reservation in status proposed.</summary>
    public static ScheduledNominationReservation NewScheduledNominationReservation(string code, string title)
    {
        return new ScheduledNominationReservation(
            code,
            title,
            0.0,
            0,
            "",
            0.0,
            ScheduledNominationReservationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledNominationReservationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledNominationReservationStatus next)
    {
        switch (Status)
        {
            case ScheduledNominationReservationStatus.Proposed:
                return next == ScheduledNominationReservationStatus.Approved || next == ScheduledNominationReservationStatus.Withdrawn;
            case ScheduledNominationReservationStatus.Approved:
                return next == ScheduledNominationReservationStatus.Rejected;
            case ScheduledNominationReservationStatus.Rejected:
                return next == ScheduledNominationReservationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledNominationReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled nomination reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Margin * Fte, 2);
    }

    public ScheduledNominationReservation Copy()
    {
        return new ScheduledNominationReservation(Code, Title, Fte, WindowDays, Category, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
