namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking snapshot is in its life cycle.</summary>
public enum BookingSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A booking snapshot. Its code is unique within a tenant.</summary>
public sealed class BookingSnapshot(string code, string name, bool approved, string notes, string description, int durationMinutes, string summary, BookingSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string Notes { get; set; } = notes;
    public string Description { get; set; } = description;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Summary { get; set; } = summary;
    public BookingSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a booking snapshot in status new.</summary>
    public static BookingSnapshot NewBookingSnapshot(string code, string name)
    {
        return new BookingSnapshot(code, name, false, "", "", 0, "", BookingSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingSnapshotStatus next)
    {
        switch (Status)
        {
            case BookingSnapshotStatus.New:
                return next == BookingSnapshotStatus.Running;
            case BookingSnapshotStatus.Queued:
                switch (next)
                {
                    case BookingSnapshotStatus.Done:
                    case BookingSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case BookingSnapshotStatus.Running:
                switch (next)
                {
                    case BookingSnapshotStatus.Done:
                        return true;
                }
                return false;
            case BookingSnapshotStatus.Done:
                switch (next)
                {
                    case BookingSnapshotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"booking snapshot"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public BookingSnapshot Copy()
    {
        return new BookingSnapshot(Code, Name, Approved, Notes, Description, DurationMinutes, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
