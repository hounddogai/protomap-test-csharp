namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled booking audit is in its life cycle.</summary>
public enum ScheduledBookingAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled booking audit as the booking team keeps it.</summary>
public sealed class ScheduledBookingAudit(string code, string title, double cost, string summary, List<string> tags, ScheduledBookingAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string Summary { get; set; } = summary;
    public List<string> Tags { get; set; } = tags;
    public ScheduledBookingAuditStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled booking audit in status planned.</summary>
    public static ScheduledBookingAudit NewScheduledBookingAudit(string code, string title)
    {
        return new ScheduledBookingAudit(
            code,
            title,
            0.0,
            "",
            new List<string>(),
            ScheduledBookingAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledBookingAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledBookingAuditStatus next)
    {
        switch (Status)
        {
            case ScheduledBookingAuditStatus.Planned:
                switch (next)
                {
                    case ScheduledBookingAuditStatus.Scheduled:
                    case ScheduledBookingAuditStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledBookingAuditStatus.Scheduled:
                return next == ScheduledBookingAuditStatus.InProgress;
            case ScheduledBookingAuditStatus.InProgress:
                switch (next)
                {
                    case ScheduledBookingAuditStatus.Completed:
                    case ScheduledBookingAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledBookingAuditStatus.Completed:
                return next == ScheduledBookingAuditStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledBookingAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled booking audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled booking audit"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public ScheduledBookingAudit Copy()
    {
        return new ScheduledBookingAudit(Code, Title, Cost, Summary, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
