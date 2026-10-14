namespace Acme.Workforce.Workplace.Booking.BookingAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking batch is in its life cycle.</summary>
public enum BookingBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A booking batch. Its code is unique within a tenant.</summary>
public sealed class BookingBatch(string code, string label, string location, int windowDays, string checksum, bool pinned, bool active, BookingBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public int WindowDays { get; set; } = windowDays;
    public string Checksum { get; set; } = checksum;
    public bool Pinned { get; set; } = pinned;
    public bool Active { get; set; } = active;
    public BookingBatchStatus Status { get; set; } = status;

    /// <summary>Starts a booking batch in status idle.</summary>
    public static BookingBatch NewBookingBatch(string code, string label)
    {
        return new BookingBatch(code, label, "", 0, "", false, false, BookingBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingBatchStatus next)
    {
        switch (Status)
        {
            case BookingBatchStatus.Idle:
                switch (next)
                {
                    case BookingBatchStatus.Warming:
                    case BookingBatchStatus.Stopped:
                        return true;
                }
                return false;
            case BookingBatchStatus.Warming:
                return next == BookingBatchStatus.Ready;
            case BookingBatchStatus.Ready:
                switch (next)
                {
                    case BookingBatchStatus.Draining:
                    case BookingBatchStatus.Stopped:
                        return true;
                }
                return false;
            case BookingBatchStatus.Draining:
                return next == BookingBatchStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BookingBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public BookingBatch Copy()
    {
        return new BookingBatch(Code, Label, Location, WindowDays, Checksum, Pinned, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
