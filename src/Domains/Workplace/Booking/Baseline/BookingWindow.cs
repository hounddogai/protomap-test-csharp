namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking window is in its life cycle.</summary>
public enum BookingWindowStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A booking window as the booking team keeps it.</summary>
public sealed class BookingWindow(string code, string name, string phone, string title, string notes, bool visible, double cost, double rate, BookingWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public bool Visible { get; set; } = visible;
    public double Cost { get; set; } = cost;
    public double Rate { get; set; } = rate;
    public BookingWindowStatus Status { get; set; } = status;

    /// <summary>Starts a booking window in status created.</summary>
    public static BookingWindow NewBookingWindow(string code, string name)
    {
        return new BookingWindow(code, name, "", "", "", false, 0.0, 0.0, BookingWindowStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingWindowStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingWindowStatus next)
    {
        switch (Status)
        {
            case BookingWindowStatus.Created:
                return next == BookingWindowStatus.Validated || next == BookingWindowStatus.Published;
            case BookingWindowStatus.Validated:
                switch (next)
                {
                    case BookingWindowStatus.Published:
                    case BookingWindowStatus.Retired:
                        return true;
                }
                return false;
            case BookingWindowStatus.Published:
                switch (next)
                {
                    case BookingWindowStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"booking window"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Rate * Cost, 2);
    }

    public BookingWindow Copy()
    {
        return new BookingWindow(Code, Name, Phone, Title, Notes, Visible, Cost, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
