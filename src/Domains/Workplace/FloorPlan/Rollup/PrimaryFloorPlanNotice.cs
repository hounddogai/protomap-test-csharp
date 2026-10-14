namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary floor plan notice is in its life cycle.</summary>
public enum PrimaryFloorPlanNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A primary floor plan notice. Its code is unique within a tenant.</summary>
public sealed class PrimaryFloorPlanNotice(string code, string label, bool pinned, bool active, int priority, bool approved, PrimaryFloorPlanNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public bool Active { get; set; } = active;
    public int Priority { get; set; } = priority;
    public bool Approved { get; set; } = approved;
    public PrimaryFloorPlanNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a primary floor plan notice in status draft.</summary>
    public static PrimaryFloorPlanNotice NewPrimaryFloorPlanNotice(string code, string label)
    {
        return new PrimaryFloorPlanNotice(code, label, false, false, 0, false, PrimaryFloorPlanNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryFloorPlanNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryFloorPlanNoticeStatus next)
    {
        switch (Status)
        {
            case PrimaryFloorPlanNoticeStatus.Draft:
                return next == PrimaryFloorPlanNoticeStatus.Suspended;
            case PrimaryFloorPlanNoticeStatus.Pending:
                switch (next)
                {
                    case PrimaryFloorPlanNoticeStatus.Active:
                    case PrimaryFloorPlanNoticeStatus.Closed:
                        return true;
                }
                return false;
            case PrimaryFloorPlanNoticeStatus.Active:
                return next == PrimaryFloorPlanNoticeStatus.Suspended;
            case PrimaryFloorPlanNoticeStatus.Suspended:
                switch (next)
                {
                    case PrimaryFloorPlanNoticeStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryFloorPlanNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary floor plan notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public PrimaryFloorPlanNotice Copy()
    {
        return new PrimaryFloorPlanNotice(Code, Label, Pinned, Active, Priority, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
