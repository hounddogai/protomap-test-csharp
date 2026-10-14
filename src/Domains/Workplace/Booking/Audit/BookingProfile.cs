namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking profile is in its life cycle.</summary>
public enum BookingProfileStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A booking profile as the booking team keeps it.</summary>
public sealed class BookingProfile(string code, string label, int revision, bool visible, string ownerId, string unit, int capacity, int expiresAt, string currency, double discount, bool approved, BookingProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public bool Visible { get; set; } = visible;
    public string OwnerId { get; set; } = ownerId;
    public string Unit { get; set; } = unit;
    public int Capacity { get; set; } = capacity;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Currency { get; set; } = currency;
    public double Discount { get; set; } = discount;
    public bool Approved { get; set; } = approved;
    public BookingProfileStatus Status { get; set; } = status;

    /// <summary>Starts a booking profile in status requested.</summary>
    public static BookingProfile NewBookingProfile(string code, string label)
    {
        return new BookingProfile(
            code,
            label,
            0,
            false,
            "",
            "",
            0,
            0,
            "",
            0.0,
            false,
            BookingProfileStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingProfileStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingProfileStatus next)
    {
        switch (Status)
        {
            case BookingProfileStatus.Requested:
                switch (next)
                {
                    case BookingProfileStatus.Granted:
                    case BookingProfileStatus.Expired:
                        return true;
                }
                return false;
            case BookingProfileStatus.Granted:
                return next == BookingProfileStatus.Revoked || next == BookingProfileStatus.Expired;
            case BookingProfileStatus.Revoked:
                switch (next)
                {
                    case BookingProfileStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"booking profile"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Discount * (double)Capacity, 2);
    }

    public BookingProfile Copy()
    {
        return new BookingProfile(
            Code,
            Label,
            Revision,
            Visible,
            OwnerId,
            Unit,
            Capacity,
            ExpiresAt,
            Currency,
            Discount,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
