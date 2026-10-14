namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking reservation is in its life cycle.</summary>
public enum BookingReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A booking reservation as the booking team keeps it.</summary>
public sealed class BookingReservation(string code, string label, int updatedAt, double maximum, int tenureMonths, double margin, double cost, int batchSize, string badge, BookingReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Margin { get; set; } = margin;
    public double Cost { get; set; } = cost;
    public int BatchSize { get; set; } = batchSize;
    public string Badge { get; set; } = badge;
    public BookingReservationStatus Status { get; set; } = status;

    /// <summary>Starts a booking reservation in status draft.</summary>
    public static BookingReservation NewBookingReservation(string code, string label)
    {
        return new BookingReservation(code, label, 0, 0.0, 0, 0.0, 0.0, 0, "", BookingReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingReservationStatus next)
    {
        switch (Status)
        {
            case BookingReservationStatus.Draft:
                return next == BookingReservationStatus.Closed;
            case BookingReservationStatus.Pending:
                switch (next)
                {
                    case BookingReservationStatus.Suspended:
                        return true;
                }
                return false;
            case BookingReservationStatus.Active:
                switch (next)
                {
                    case BookingReservationStatus.Suspended:
                        return true;
                }
                return false;
            case BookingReservationStatus.Suspended:
                return next == BookingReservationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BookingReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)TenureMonths * (double)UpdatedAt, 2);
    }

    public BookingReservation Copy()
    {
        return new BookingReservation(
            Code,
            Label,
            UpdatedAt,
            Maximum,
            TenureMonths,
            Margin,
            Cost,
            BatchSize,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
