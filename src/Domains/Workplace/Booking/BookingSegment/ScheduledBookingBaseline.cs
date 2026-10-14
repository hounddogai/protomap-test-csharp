namespace Acme.Workforce.Workplace.Booking.BookingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled booking baseline is in its life cycle.</summary>
public enum ScheduledBookingBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scheduled booking baseline, with what the planners need to act on it.</summary>
public sealed class ScheduledBookingBaseline(string code, string title, string team, string summary, bool archived, int limit, int durationMinutes, ScheduledBookingBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Summary { get; set; } = summary;
    public bool Archived { get; set; } = archived;
    public int Limit { get; set; } = limit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ScheduledBookingBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled booking baseline in status planned.</summary>
    public static ScheduledBookingBaseline NewScheduledBookingBaseline(string code, string title)
    {
        return new ScheduledBookingBaseline(
            code,
            title,
            "",
            "",
            false,
            0,
            0,
            ScheduledBookingBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBookingBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBookingBaselineStatus next)
    {
        switch (Status)
        {
            case ScheduledBookingBaselineStatus.Planned:
                return next == ScheduledBookingBaselineStatus.Scheduled || next == ScheduledBookingBaselineStatus.Completed;
            case ScheduledBookingBaselineStatus.Scheduled:
                switch (next)
                {
                    case ScheduledBookingBaselineStatus.InProgress:
                    case ScheduledBookingBaselineStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledBookingBaselineStatus.InProgress:
                return next == ScheduledBookingBaselineStatus.Completed || next == ScheduledBookingBaselineStatus.Cancelled;
            case ScheduledBookingBaselineStatus.Completed:
                return next == ScheduledBookingBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledBookingBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled booking baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Limit - (double)DurationMinutes, 2);
    }

    public ScheduledBookingBaseline Copy()
    {
        return new ScheduledBookingBaseline(Code, Title, Team, Summary, Archived, Limit, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
