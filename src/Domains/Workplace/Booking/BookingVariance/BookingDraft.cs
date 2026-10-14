namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking draft is in its life cycle.</summary>
public enum BookingDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a booking draft and its current status.</summary>
public sealed class BookingDraft(string code, string title, string timezone, bool approved, string phone, string costCenter, double discount, string label, BookingDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public bool Approved { get; set; } = approved;
    public string Phone { get; set; } = phone;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public string Label { get; set; } = label;
    public BookingDraftStatus Status { get; set; } = status;

    /// <summary>Starts a booking draft in status created.</summary>
    public static BookingDraft NewBookingDraft(string code, string title)
    {
        return new BookingDraft(code, title, "", false, "", "", 0.0, "", BookingDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingDraftStatus next)
    {
        switch (Status)
        {
            case BookingDraftStatus.Created:
                switch (next)
                {
                    case BookingDraftStatus.Validated:
                    case BookingDraftStatus.Published:
                        return true;
                }
                return false;
            case BookingDraftStatus.Validated:
                return next == BookingDraftStatus.Published || next == BookingDraftStatus.Retired;
            case BookingDraftStatus.Published:
                return next == BookingDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BookingDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public BookingDraft Copy()
    {
        return new BookingDraft(Code, Title, Timezone, Approved, Phone, CostCenter, Discount, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
