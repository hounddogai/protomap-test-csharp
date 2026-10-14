namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking request is in its life cycle.</summary>
public enum BookingRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A booking request. Its code is unique within a tenant.</summary>
public sealed class BookingRequest(string code, string title, int revision, int tenureMonths, double cost, double threshold, string currency, int capacity, string name, BookingRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public double Threshold { get; set; } = threshold;
    public string Currency { get; set; } = currency;
    public int Capacity { get; set; } = capacity;
    public string Name { get; set; } = name;
    public BookingRequestStatus Status { get; set; } = status;

    /// <summary>Starts a booking request in status open.</summary>
    public static BookingRequest NewBookingRequest(string code, string title)
    {
        return new BookingRequest(code, title, 0, 0, 0.0, 0.0, "", 0, "", BookingRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingRequestStatus next)
    {
        switch (Status)
        {
            case BookingRequestStatus.Open:
                return next == BookingRequestStatus.Reopened;
            case BookingRequestStatus.InReview:
                return next == BookingRequestStatus.Archived;
            case BookingRequestStatus.Resolved:
                switch (next)
                {
                    case BookingRequestStatus.Reopened:
                        return true;
                }
                return false;
            case BookingRequestStatus.Reopened:
                switch (next)
                {
                    case BookingRequestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Revision - (double)Capacity, 2);
    }

    public BookingRequest Copy()
    {
        return new BookingRequest(
            Code,
            Title,
            Revision,
            TenureMonths,
            Cost,
            Threshold,
            Currency,
            Capacity,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
