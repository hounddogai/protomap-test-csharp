namespace Acme.Workforce.Workplace.Occupancy.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict occupancy segment is in its life cycle.</summary>
public enum StrictOccupancySegmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a strict occupancy segment and its current status.</summary>
public sealed class StrictOccupancySegment(string code, string label, double amount, string managerId, string region, StrictOccupancySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public string Region { get; set; } = region;
    public StrictOccupancySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a strict occupancy segment in status open.</summary>
    public static StrictOccupancySegment NewStrictOccupancySegment(string code, string label)
    {
        return new StrictOccupancySegment(code, label, 0.0, "", "", StrictOccupancySegmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictOccupancySegmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictOccupancySegmentStatus next)
    {
        switch (Status)
        {
            case StrictOccupancySegmentStatus.Open:
                return next == StrictOccupancySegmentStatus.Resolved;
            case StrictOccupancySegmentStatus.InReview:
                return next == StrictOccupancySegmentStatus.Resolved || next == StrictOccupancySegmentStatus.Reopened;
            case StrictOccupancySegmentStatus.Resolved:
                switch (next)
                {
                    case StrictOccupancySegmentStatus.Archived:
                        return true;
                }
                return false;
            case StrictOccupancySegmentStatus.Reopened:
                return next == StrictOccupancySegmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictOccupancySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict occupancy segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public StrictOccupancySegment Copy()
    {
        return new StrictOccupancySegment(Code, Label, Amount, ManagerId, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
