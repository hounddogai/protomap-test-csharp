namespace Acme.Workforce.Workplace.Booking.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking checkpoint is in its life cycle.</summary>
public enum BookingCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One booking checkpoint, with what the planners need to act on it.</summary>
public sealed class BookingCheckpoint(string code, string label, double hours, string checksum, bool pinned, int capacity, string category, BookingCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string Checksum { get; set; } = checksum;
    public bool Pinned { get; set; } = pinned;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public BookingCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a booking checkpoint in status open.</summary>
    public static BookingCheckpoint NewBookingCheckpoint(string code, string label)
    {
        return new BookingCheckpoint(code, label, 0.0, "", false, 0, "", BookingCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingCheckpointStatus next)
    {
        switch (Status)
        {
            case BookingCheckpointStatus.Open:
                switch (next)
                {
                    case BookingCheckpointStatus.Reopened:
                    case BookingCheckpointStatus.Archived:
                        return true;
                }
                return false;
            case BookingCheckpointStatus.InReview:
                return next == BookingCheckpointStatus.Reopened;
            case BookingCheckpointStatus.Resolved:
                switch (next)
                {
                    case BookingCheckpointStatus.Reopened:
                        return true;
                }
                return false;
            case BookingCheckpointStatus.Reopened:
                switch (next)
                {
                    case BookingCheckpointStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Capacity * Hours, 2);
    }

    public BookingCheckpoint Copy()
    {
        return new BookingCheckpoint(Code, Label, Hours, Checksum, Pinned, Capacity, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
