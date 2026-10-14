namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking segment is in its life cycle.</summary>
public enum BookingSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A booking segment as the booking team keeps it.</summary>
public sealed class BookingSegment(string code, string name, int createdAt, int leaveDays, double ceiling, string employeeId, BookingSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public int LeaveDays { get; set; } = leaveDays;
    public double Ceiling { get; set; } = ceiling;
    public string EmployeeId { get; set; } = employeeId;
    public BookingSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a booking segment in status new.</summary>
    public static BookingSegment NewBookingSegment(string code, string name)
    {
        return new BookingSegment(code, name, 0, 0, 0.0, "", BookingSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingSegmentStatus next)
    {
        switch (Status)
        {
            case BookingSegmentStatus.New:
                return next == BookingSegmentStatus.Queued;
            case BookingSegmentStatus.Queued:
                return next == BookingSegmentStatus.Running;
            case BookingSegmentStatus.Running:
                return next == BookingSegmentStatus.Done || next == BookingSegmentStatus.Failed;
            case BookingSegmentStatus.Done:
                return next == BookingSegmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BookingSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"booking segment"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)LeaveDays - Ceiling, 2);
    }

    public BookingSegment Copy()
    {
        return new BookingSegment(Code, Name, CreatedAt, LeaveDays, Ceiling, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
