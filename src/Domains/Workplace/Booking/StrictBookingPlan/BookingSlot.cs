namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking slot is in its life cycle.</summary>
public enum BookingSlotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One booking slot, with what the planners need to act on it.</summary>
public sealed class BookingSlot(string code, string title, bool visible, double rating, string channel, string contactEmail, double cost, int capacity, string ownerId, int tenureMonths, string badge, BookingSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public double Rating { get; set; } = rating;
    public string Channel { get; set; } = channel;
    public string ContactEmail { get; set; } = contactEmail;
    public double Cost { get; set; } = cost;
    public int Capacity { get; set; } = capacity;
    public string OwnerId { get; set; } = ownerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public BookingSlotStatus Status { get; set; } = status;

    /// <summary>Starts a booking slot in status created.</summary>
    public static BookingSlot NewBookingSlot(string code, string title)
    {
        return new BookingSlot(code, title, false, 0.0, "", "", 0.0, 0, "", 0, "", BookingSlotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingSlotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingSlotStatus next)
    {
        switch (Status)
        {
            case BookingSlotStatus.Created:
                return next == BookingSlotStatus.Retired;
            case BookingSlotStatus.Validated:
                switch (next)
                {
                    case BookingSlotStatus.Published:
                        return true;
                }
                return false;
            case BookingSlotStatus.Published:
                return next == BookingSlotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BookingSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"booking slot"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Cost - (double)TenureMonths, 2);
    }

    public BookingSlot Copy()
    {
        return new BookingSlot(
            Code,
            Title,
            Visible,
            Rating,
            Channel,
            ContactEmail,
            Cost,
            Capacity,
            OwnerId,
            TenureMonths,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
