namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking digest is in its life cycle.</summary>
public enum BookingDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One booking digest, with what the planners need to act on it.</summary>
public sealed class BookingDigest(string code, string name, string employeeId, int graceDays, int capacity, string locale, BookingDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int GraceDays { get; set; } = graceDays;
    public int Capacity { get; set; } = capacity;
    public string Locale { get; set; } = locale;
    public BookingDigestStatus Status { get; set; } = status;

    /// <summary>Starts a booking digest in status idle.</summary>
    public static BookingDigest NewBookingDigest(string code, string name)
    {
        return new BookingDigest(code, name, "", 0, 0, "", BookingDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingDigestStatus next)
    {
        switch (Status)
        {
            case BookingDigestStatus.Idle:
                switch (next)
                {
                    case BookingDigestStatus.Warming:
                    case BookingDigestStatus.Draining:
                        return true;
                }
                return false;
            case BookingDigestStatus.Warming:
                return next == BookingDigestStatus.Draining;
            case BookingDigestStatus.Ready:
                return next == BookingDigestStatus.Draining || next == BookingDigestStatus.Stopped;
            case BookingDigestStatus.Draining:
                return next == BookingDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BookingDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)GraceDays * (double)Capacity, 2);
    }

    public BookingDigest Copy()
    {
        return new BookingDigest(Code, Name, EmployeeId, GraceDays, Capacity, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
