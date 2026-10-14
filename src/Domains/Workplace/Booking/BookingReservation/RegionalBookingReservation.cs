namespace Acme.Workforce.Workplace.Booking.BookingReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional booking reservation is in its life cycle.</summary>
public enum RegionalBookingReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional booking reservation as the booking team keeps it.</summary>
public sealed class RegionalBookingReservation(string code, string name, double weight, int startsAt, string ownerId, double maximum, string description, int limit, int durationMinutes, RegionalBookingReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public int StartsAt { get; set; } = startsAt;
    public string OwnerId { get; set; } = ownerId;
    public double Maximum { get; set; } = maximum;
    public string Description { get; set; } = description;
    public int Limit { get; set; } = limit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public RegionalBookingReservationStatus Status { get; set; } = status;

    /// <summary>Starts a regional booking reservation in status open.</summary>
    public static RegionalBookingReservation NewRegionalBookingReservation(string code, string name)
    {
        return new RegionalBookingReservation(
            code,
            name,
            0.0,
            0,
            "",
            0.0,
            "",
            0,
            0,
            RegionalBookingReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBookingReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBookingReservationStatus next)
    {
        switch (Status)
        {
            case RegionalBookingReservationStatus.Open:
                switch (next)
                {
                    case RegionalBookingReservationStatus.InReview:
                    case RegionalBookingReservationStatus.Resolved:
                        return true;
                }
                return false;
            case RegionalBookingReservationStatus.InReview:
                switch (next)
                {
                    case RegionalBookingReservationStatus.Resolved:
                    case RegionalBookingReservationStatus.Archived:
                        return true;
                }
                return false;
            case RegionalBookingReservationStatus.Resolved:
                return next == RegionalBookingReservationStatus.Reopened;
            case RegionalBookingReservationStatus.Reopened:
                return next == RegionalBookingReservationStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RegionalBookingReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional booking reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"regional booking reservation"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Limit * (double)DurationMinutes, 2);
    }

    public RegionalBookingReservation Copy()
    {
        return new RegionalBookingReservation(
            Code,
            Name,
            Weight,
            StartsAt,
            OwnerId,
            Maximum,
            Description,
            Limit,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
