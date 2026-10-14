namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy checkpoint is in its life cycle.</summary>
public enum OccupancyCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a occupancy checkpoint and its current status.</summary>
public sealed class OccupancyCheckpoint(string code, string name, int endsAt, string locale, double minimum, string salaryBand, double floorValue, string location, string category, OccupancyCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public string Locale { get; set; } = locale;
    public double Minimum { get; set; } = minimum;
    public string SalaryBand { get; set; } = salaryBand;
    public double FloorValue { get; set; } = floorValue;
    public string Location { get; set; } = location;
    public string Category { get; set; } = category;
    public OccupancyCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy checkpoint in status planned.</summary>
    public static OccupancyCheckpoint NewOccupancyCheckpoint(string code, string name)
    {
        return new OccupancyCheckpoint(code, name, 0, "", 0.0, "", 0.0, "", "", OccupancyCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyCheckpointStatus next)
    {
        switch (Status)
        {
            case OccupancyCheckpointStatus.Planned:
                return next == OccupancyCheckpointStatus.InProgress || next == OccupancyCheckpointStatus.Cancelled;
            case OccupancyCheckpointStatus.Scheduled:
                return next == OccupancyCheckpointStatus.Cancelled;
            case OccupancyCheckpointStatus.InProgress:
                return next == OccupancyCheckpointStatus.Completed || next == OccupancyCheckpointStatus.Cancelled;
            case OccupancyCheckpointStatus.Completed:
                switch (next)
                {
                    case OccupancyCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Minimum + (double)EndsAt, 2);
    }

    public OccupancyCheckpoint Copy()
    {
        return new OccupancyCheckpoint(
            Code,
            Name,
            EndsAt,
            Locale,
            Minimum,
            SalaryBand,
            FloorValue,
            Location,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
