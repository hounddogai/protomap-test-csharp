namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy reservation is in its life cycle.</summary>
public enum OccupancyReservationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A occupancy reservation as the occupancy team keeps it.</summary>
public sealed class OccupancyReservation(string code, string title, int expiresAt, bool visible, double ceiling, OccupancyReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Visible { get; set; } = visible;
    public double Ceiling { get; set; } = ceiling;
    public OccupancyReservationStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy reservation in status created.</summary>
    public static OccupancyReservation NewOccupancyReservation(string code, string title)
    {
        return new OccupancyReservation(code, title, 0, false, 0.0, OccupancyReservationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyReservationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyReservationStatus next)
    {
        switch (Status)
        {
            case OccupancyReservationStatus.Created:
                switch (next)
                {
                    case OccupancyReservationStatus.Validated:
                        return true;
                }
                return false;
            case OccupancyReservationStatus.Validated:
                switch (next)
                {
                    case OccupancyReservationStatus.Published:
                    case OccupancyReservationStatus.Retired:
                        return true;
                }
                return false;
            case OccupancyReservationStatus.Published:
                switch (next)
                {
                    case OccupancyReservationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Ceiling + (double)ExpiresAt, 2);
    }

    public OccupancyReservation Copy()
    {
        return new OccupancyReservation(Code, Title, ExpiresAt, Visible, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
