namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking target is in its life cycle.</summary>
public enum BookingTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a booking target and its current status.</summary>
public sealed class BookingTarget(string code, string title, string ownerId, bool pinned, string region, int tenureMonths, BookingTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public bool Pinned { get; set; } = pinned;
    public string Region { get; set; } = region;
    public int TenureMonths { get; set; } = tenureMonths;
    public BookingTargetStatus Status { get; set; } = status;

    /// <summary>Starts a booking target in status new.</summary>
    public static BookingTarget NewBookingTarget(string code, string title)
    {
        return new BookingTarget(code, title, "", false, "", 0, BookingTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingTargetStatus next)
    {
        switch (Status)
        {
            case BookingTargetStatus.New:
                return next == BookingTargetStatus.Queued;
            case BookingTargetStatus.Queued:
                return next == BookingTargetStatus.Failed;
            case BookingTargetStatus.Running:
                switch (next)
                {
                    case BookingTargetStatus.Done:
                    case BookingTargetStatus.Failed:
                        return true;
                }
                return false;
            case BookingTargetStatus.Done:
                switch (next)
                {
                    case BookingTargetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public BookingTarget Copy()
    {
        return new BookingTarget(Code, Title, OwnerId, Pinned, Region, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
