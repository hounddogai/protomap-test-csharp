namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling mobility reservation is in its life cycle.</summary>
public enum RollingMobilityReservationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A rolling mobility reservation. Its code is unique within a tenant.</summary>
public sealed class RollingMobilityReservation(string code, string title, int createdAt, int graceDays, int endsAt, double ceiling, double weight, RollingMobilityReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public int GraceDays { get; set; } = graceDays;
    public int EndsAt { get; set; } = endsAt;
    public double Ceiling { get; set; } = ceiling;
    public double Weight { get; set; } = weight;
    public RollingMobilityReservationStatus Status { get; set; } = status;

    /// <summary>Starts a rolling mobility reservation in status draft.</summary>
    public static RollingMobilityReservation NewRollingMobilityReservation(string code, string title)
    {
        return new RollingMobilityReservation(
            code,
            title,
            0,
            0,
            0,
            0.0,
            0.0,
            RollingMobilityReservationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMobilityReservationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMobilityReservationStatus next)
    {
        switch (Status)
        {
            case RollingMobilityReservationStatus.Draft:
                switch (next)
                {
                    case RollingMobilityReservationStatus.Pending:
                    case RollingMobilityReservationStatus.Closed:
                        return true;
                }
                return false;
            case RollingMobilityReservationStatus.Pending:
                switch (next)
                {
                    case RollingMobilityReservationStatus.Suspended:
                    case RollingMobilityReservationStatus.Closed:
                        return true;
                }
                return false;
            case RollingMobilityReservationStatus.Active:
                return next == RollingMobilityReservationStatus.Suspended || next == RollingMobilityReservationStatus.Closed;
            case RollingMobilityReservationStatus.Suspended:
                switch (next)
                {
                    case RollingMobilityReservationStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingMobilityReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling mobility reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"rolling mobility reservation"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)CreatedAt * (double)GraceDays, 2);
    }

    public RollingMobilityReservation Copy()
    {
        return new RollingMobilityReservation(Code, Title, CreatedAt, GraceDays, EndsAt, Ceiling, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
