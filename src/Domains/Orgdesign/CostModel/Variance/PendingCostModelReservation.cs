namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending cost model reservation is in its life cycle.</summary>
public enum PendingCostModelReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending cost model reservation. Its code is unique within a tenant.</summary>
public sealed class PendingCostModelReservation(string code, string name, int tenureMonths, int durationMinutes, bool visible, int version, double floorValue, double minimum, string country, int priority, PendingCostModelReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Visible { get; set; } = visible;
    public int Version { get; set; } = version;
    public double FloorValue { get; set; } = floorValue;
    public double Minimum { get; set; } = minimum;
    public string Country { get; set; } = country;
    public int Priority { get; set; } = priority;
    public PendingCostModelReservationStatus Status { get; set; } = status;

    /// <summary>Starts a pending cost model reservation in status requested.</summary>
    public static PendingCostModelReservation NewPendingCostModelReservation(string code, string name)
    {
        return new PendingCostModelReservation(
            code,
            name,
            0,
            0,
            false,
            0,
            0.0,
            0.0,
            "",
            0,
            PendingCostModelReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingCostModelReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingCostModelReservationStatus next)
    {
        switch (Status)
        {
            case PendingCostModelReservationStatus.Requested:
                switch (next)
                {
                    case PendingCostModelReservationStatus.Expired:
                        return true;
                }
                return false;
            case PendingCostModelReservationStatus.Granted:
                switch (next)
                {
                    case PendingCostModelReservationStatus.Expired:
                        return true;
                }
                return false;
            case PendingCostModelReservationStatus.Revoked:
                switch (next)
                {
                    case PendingCostModelReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingCostModelReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending cost model reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)DurationMinutes * (double)Version, 2);
    }

    public PendingCostModelReservation Copy()
    {
        return new PendingCostModelReservation(
            Code,
            Name,
            TenureMonths,
            DurationMinutes,
            Visible,
            Version,
            FloorValue,
            Minimum,
            Country,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
