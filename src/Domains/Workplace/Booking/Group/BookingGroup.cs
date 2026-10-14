namespace Acme.Workforce.Workplace.Booking.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a booking group is in its life cycle.</summary>
public enum BookingGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a booking group and its current status.</summary>
public sealed class BookingGroup(string code, string name, string ownerId, double weight, int startsAt, int batchSize, int createdAt, int windowDays, double budget, double discount, BookingGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public double Weight { get; set; } = weight;
    public int StartsAt { get; set; } = startsAt;
    public int BatchSize { get; set; } = batchSize;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public double Budget { get; set; } = budget;
    public double Discount { get; set; } = discount;
    public BookingGroupStatus Status { get; set; } = status;

    /// <summary>Starts a booking group in status created.</summary>
    public static BookingGroup NewBookingGroup(string code, string name)
    {
        return new BookingGroup(code, name, "", 0.0, 0, 0, 0, 0, 0.0, 0.0, BookingGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BookingGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BookingGroupStatus next)
    {
        switch (Status)
        {
            case BookingGroupStatus.Created:
                return next == BookingGroupStatus.Validated;
            case BookingGroupStatus.Validated:
                return next == BookingGroupStatus.Retired;
            case BookingGroupStatus.Published:
                switch (next)
                {
                    case BookingGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BookingGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"booking group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Budget - Discount, 2);
    }

    public BookingGroup Copy()
    {
        return new BookingGroup(
            Code,
            Name,
            OwnerId,
            Weight,
            StartsAt,
            BatchSize,
            CreatedAt,
            WindowDays,
            Budget,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
