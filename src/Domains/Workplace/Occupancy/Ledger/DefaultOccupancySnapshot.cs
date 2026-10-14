namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default occupancy snapshot is in its life cycle.</summary>
public enum DefaultOccupancySnapshotStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default occupancy snapshot as the occupancy team keeps it.</summary>
public sealed class DefaultOccupancySnapshot(string code, string label, string location, string summary, double fte, string category, double floorValue, int limit, bool visible, DefaultOccupancySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public string Summary { get; set; } = summary;
    public double Fte { get; set; } = fte;
    public string Category { get; set; } = category;
    public double FloorValue { get; set; } = floorValue;
    public int Limit { get; set; } = limit;
    public bool Visible { get; set; } = visible;
    public DefaultOccupancySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a default occupancy snapshot in status proposed.</summary>
    public static DefaultOccupancySnapshot NewDefaultOccupancySnapshot(string code, string label)
    {
        return new DefaultOccupancySnapshot(
            code,
            label,
            "",
            "",
            0.0,
            "",
            0.0,
            0,
            false,
            DefaultOccupancySnapshotStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultOccupancySnapshotStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultOccupancySnapshotStatus next)
    {
        switch (Status)
        {
            case DefaultOccupancySnapshotStatus.Proposed:
                switch (next)
                {
                    case DefaultOccupancySnapshotStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultOccupancySnapshotStatus.Approved:
                return next == DefaultOccupancySnapshotStatus.Rejected || next == DefaultOccupancySnapshotStatus.Withdrawn;
            case DefaultOccupancySnapshotStatus.Rejected:
                switch (next)
                {
                    case DefaultOccupancySnapshotStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultOccupancySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default occupancy snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default occupancy snapshot"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(FloorValue - Fte, 2);
    }

    public DefaultOccupancySnapshot Copy()
    {
        return new DefaultOccupancySnapshot(
            Code,
            Label,
            Location,
            Summary,
            Fte,
            Category,
            FloorValue,
            Limit,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
