namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient floor plan audit is in its life cycle.</summary>
public enum LenientFloorPlanAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a lenient floor plan audit and its current status.</summary>
public sealed class LenientFloorPlanAudit(string code, string name, int quantity, int leaveDays, int batchSize, LenientFloorPlanAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public int LeaveDays { get; set; } = leaveDays;
    public int BatchSize { get; set; } = batchSize;
    public LenientFloorPlanAuditStatus Status { get; set; } = status;

    /// <summary>Starts a lenient floor plan audit in status open.</summary>
    public static LenientFloorPlanAudit NewLenientFloorPlanAudit(string code, string name)
    {
        return new LenientFloorPlanAudit(code, name, 0, 0, 0, LenientFloorPlanAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientFloorPlanAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientFloorPlanAuditStatus next)
    {
        switch (Status)
        {
            case LenientFloorPlanAuditStatus.Open:
                return next == LenientFloorPlanAuditStatus.Reopened;
            case LenientFloorPlanAuditStatus.InReview:
                return next == LenientFloorPlanAuditStatus.Reopened;
            case LenientFloorPlanAuditStatus.Resolved:
                switch (next)
                {
                    case LenientFloorPlanAuditStatus.Reopened:
                    case LenientFloorPlanAuditStatus.Archived:
                        return true;
                }
                return false;
            case LenientFloorPlanAuditStatus.Reopened:
                return next == LenientFloorPlanAuditStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientFloorPlanAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient floor plan audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Quantity + (double)LeaveDays, 2);
    }

    public LenientFloorPlanAudit Copy()
    {
        return new LenientFloorPlanAudit(Code, Name, Quantity, LeaveDays, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
