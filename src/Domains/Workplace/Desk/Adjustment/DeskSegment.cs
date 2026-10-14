namespace Acme.Workforce.Workplace.Desk.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk segment is in its life cycle.</summary>
public enum DeskSegmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A desk segment. Its code is unique within a tenant.</summary>
public sealed class DeskSegment(string code, string title, double floorValue, string managerId, string unit, string badge, string location, DeskSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public string ManagerId { get; set; } = managerId;
    public string Unit { get; set; } = unit;
    public string Badge { get; set; } = badge;
    public string Location { get; set; } = location;
    public DeskSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a desk segment in status proposed.</summary>
    public static DeskSegment NewDeskSegment(string code, string title)
    {
        return new DeskSegment(code, title, 0.0, "", "", "", "", DeskSegmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskSegmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskSegmentStatus next)
    {
        switch (Status)
        {
            case DeskSegmentStatus.Proposed:
                switch (next)
                {
                    case DeskSegmentStatus.Rejected:
                    case DeskSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeskSegmentStatus.Approved:
                return next == DeskSegmentStatus.Rejected;
            case DeskSegmentStatus.Rejected:
                switch (next)
                {
                    case DeskSegmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public DeskSegment Copy()
    {
        return new DeskSegment(Code, Title, FloorValue, ManagerId, Unit, Badge, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
