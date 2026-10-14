namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy schedule is in its life cycle.</summary>
public enum OccupancyScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A occupancy schedule. Its code is unique within a tenant.</summary>
public sealed class OccupancySchedule(string code, string title, string currency, int limit, int sequence, int startsAt, string department, string region, string location, bool archived, int quantity, OccupancyScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public int Limit { get; set; } = limit;
    public int Sequence { get; set; } = sequence;
    public int StartsAt { get; set; } = startsAt;
    public string Department { get; set; } = department;
    public string Region { get; set; } = region;
    public string Location { get; set; } = location;
    public bool Archived { get; set; } = archived;
    public int Quantity { get; set; } = quantity;
    public OccupancyScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy schedule in status planned.</summary>
    public static OccupancySchedule NewOccupancySchedule(string code, string title)
    {
        return new OccupancySchedule(
            code,
            title,
            "",
            0,
            0,
            0,
            "",
            "",
            "",
            false,
            0,
            OccupancyScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyScheduleStatus next)
    {
        switch (Status)
        {
            case OccupancyScheduleStatus.Planned:
                return next == OccupancyScheduleStatus.Scheduled || next == OccupancyScheduleStatus.Cancelled;
            case OccupancyScheduleStatus.Scheduled:
                return next == OccupancyScheduleStatus.Cancelled;
            case OccupancyScheduleStatus.InProgress:
                return next == OccupancyScheduleStatus.Completed;
            case OccupancyScheduleStatus.Completed:
                return next == OccupancyScheduleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(OccupancyScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)StartsAt - (double)Sequence, 2);
    }

    public OccupancySchedule Copy()
    {
        return new OccupancySchedule(
            Code,
            Title,
            Currency,
            Limit,
            Sequence,
            StartsAt,
            Department,
            Region,
            Location,
            Archived,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
