namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking agreement is in its life cycle.</summary>
public enum BookingAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a booking agreement and its current status.</summary>
public sealed class BookingAgreement(string code, string name, string country, string category, string notes, int windowDays, string currency, int tenureMonths, double cost, BookingAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public string Notes { get; set; } = notes;
    public int WindowDays { get; set; } = windowDays;
    public string Currency { get; set; } = currency;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public BookingAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a booking agreement in status open.</summary>
    public static BookingAgreement NewBookingAgreement(string code, string name)
    {
        return new BookingAgreement(code, name, "", "", "", 0, "", 0, 0.0, BookingAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingAgreementStatus next)
    {
        switch (Status)
        {
            case BookingAgreementStatus.Open:
                switch (next)
                {
                    case BookingAgreementStatus.Reopened:
                    case BookingAgreementStatus.Archived:
                        return true;
                }
                return false;
            case BookingAgreementStatus.InReview:
                switch (next)
                {
                    case BookingAgreementStatus.Resolved:
                    case BookingAgreementStatus.Archived:
                        return true;
                }
                return false;
            case BookingAgreementStatus.Resolved:
                return next == BookingAgreementStatus.Archived;
            case BookingAgreementStatus.Reopened:
                switch (next)
                {
                    case BookingAgreementStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetLoad()
    {
        return Math.Round((double)TenureMonths + (double)WindowDays, 2);
    }

    public BookingAgreement Copy()
    {
        return new BookingAgreement(
            Code,
            Name,
            Country,
            Category,
            Notes,
            WindowDays,
            Currency,
            TenureMonths,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
