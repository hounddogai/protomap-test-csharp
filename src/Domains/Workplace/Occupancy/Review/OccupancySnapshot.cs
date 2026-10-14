namespace Acme.Workforce.Workplace.Occupancy.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy snapshot is in its life cycle.</summary>
public enum OccupancySnapshotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One occupancy snapshot, with what the planners need to act on it.</summary>
public sealed class OccupancySnapshot(string code, string title, string checksum, string segment, string badge, int quantity, OccupancySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string Segment { get; set; } = segment;
    public string Badge { get; set; } = badge;
    public int Quantity { get; set; } = quantity;
    public OccupancySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy snapshot in status open.</summary>
    public static OccupancySnapshot NewOccupancySnapshot(string code, string title)
    {
        return new OccupancySnapshot(code, title, "", "", "", 0, OccupancySnapshotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancySnapshotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancySnapshotStatus next)
    {
        switch (Status)
        {
            case OccupancySnapshotStatus.Open:
                return next == OccupancySnapshotStatus.InReview || next == OccupancySnapshotStatus.Reopened;
            case OccupancySnapshotStatus.InReview:
                return next == OccupancySnapshotStatus.Reopened || next == OccupancySnapshotStatus.Archived;
            case OccupancySnapshotStatus.Resolved:
                return next == OccupancySnapshotStatus.Archived;
            case OccupancySnapshotStatus.Reopened:
                return next == OccupancySnapshotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(OccupancySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public OccupancySnapshot Copy()
    {
        return new OccupancySnapshot(Code, Title, Checksum, Segment, Badge, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
