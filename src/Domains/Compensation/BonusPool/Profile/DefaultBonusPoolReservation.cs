namespace Acme.Workforce.Compensation.BonusPool.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default bonus pool reservation is in its life cycle.</summary>
public enum DefaultBonusPoolReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default bonus pool reservation as the bonus pool team keeps it.</summary>
public sealed class DefaultBonusPoolReservation(string code, string name, double budget, double ratio, int updatedAt, string unit, string costCenter, bool active, double floorValue, int revision, double minimum, DefaultBonusPoolReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public double Ratio { get; set; } = ratio;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Unit { get; set; } = unit;
    public string CostCenter { get; set; } = costCenter;
    public bool Active { get; set; } = active;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public double Minimum { get; set; } = minimum;
    public DefaultBonusPoolReservationStatus Status { get; set; } = status;

    /// <summary>Starts a default bonus pool reservation in status planned.</summary>
    public static DefaultBonusPoolReservation NewDefaultBonusPoolReservation(string code, string name)
    {
        return new DefaultBonusPoolReservation(
            code,
            name,
            0.0,
            0.0,
            0,
            "",
            "",
            false,
            0.0,
            0,
            0.0,
            DefaultBonusPoolReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultBonusPoolReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultBonusPoolReservationStatus next)
    {
        switch (Status)
        {
            case DefaultBonusPoolReservationStatus.Planned:
                return next == DefaultBonusPoolReservationStatus.Scheduled || next == DefaultBonusPoolReservationStatus.InProgress;
            case DefaultBonusPoolReservationStatus.Scheduled:
                switch (next)
                {
                    case DefaultBonusPoolReservationStatus.Completed:
                    case DefaultBonusPoolReservationStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultBonusPoolReservationStatus.InProgress:
                return next == DefaultBonusPoolReservationStatus.Cancelled;
            case DefaultBonusPoolReservationStatus.Completed:
                switch (next)
                {
                    case DefaultBonusPoolReservationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultBonusPoolReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default bonus pool reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"default bonus pool reservation"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Minimum - Ratio, 2);
    }

    public DefaultBonusPoolReservation Copy()
    {
        return new DefaultBonusPoolReservation(
            Code,
            Name,
            Budget,
            Ratio,
            UpdatedAt,
            Unit,
            CostCenter,
            Active,
            FloorValue,
            Revision,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
