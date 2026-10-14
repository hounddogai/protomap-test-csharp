namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled occupancy snapshot is in its life cycle.</summary>
public enum ScheduledOccupancySnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled occupancy snapshot. Its code is unique within a tenant.</summary>
public sealed class ScheduledOccupancySnapshot(string code, string name, bool archived, int revision, bool pinned, double cost, string grade, ScheduledOccupancySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public int Revision { get; set; } = revision;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public ScheduledOccupancySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled occupancy snapshot in status created.</summary>
    public static ScheduledOccupancySnapshot NewScheduledOccupancySnapshot(string code, string name)
    {
        return new ScheduledOccupancySnapshot(
            code,
            name,
            false,
            0,
            false,
            0.0,
            "",
            ScheduledOccupancySnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledOccupancySnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledOccupancySnapshotStatus next)
    {
        switch (Status)
        {
            case ScheduledOccupancySnapshotStatus.Created:
                return next == ScheduledOccupancySnapshotStatus.Validated;
            case ScheduledOccupancySnapshotStatus.Validated:
                switch (next)
                {
                    case ScheduledOccupancySnapshotStatus.Published:
                    case ScheduledOccupancySnapshotStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledOccupancySnapshotStatus.Published:
                return next == ScheduledOccupancySnapshotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledOccupancySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled occupancy snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Cost * (double)Revision, 2);
    }

    public ScheduledOccupancySnapshot Copy()
    {
        return new ScheduledOccupancySnapshot(Code, Name, Archived, Revision, Pinned, Cost, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
