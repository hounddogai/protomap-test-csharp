namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan audit is in its life cycle.</summary>
public enum FloorPlanAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A floor plan audit. Its code is unique within a tenant.</summary>
public sealed class FloorPlanAudit(string code, string title, string currency, double ceiling, string region, double discount, bool visible, FloorPlanAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Ceiling { get; set; } = ceiling;
    public string Region { get; set; } = region;
    public double Discount { get; set; } = discount;
    public bool Visible { get; set; } = visible;
    public FloorPlanAuditStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan audit in status planned.</summary>
    public static FloorPlanAudit NewFloorPlanAudit(string code, string title)
    {
        return new FloorPlanAudit(code, title, "", 0.0, "", 0.0, false, FloorPlanAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanAuditStatus next)
    {
        switch (Status)
        {
            case FloorPlanAuditStatus.Planned:
                switch (next)
                {
                    case FloorPlanAuditStatus.Scheduled:
                    case FloorPlanAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case FloorPlanAuditStatus.Scheduled:
                switch (next)
                {
                    case FloorPlanAuditStatus.InProgress:
                        return true;
                }
                return false;
            case FloorPlanAuditStatus.InProgress:
                switch (next)
                {
                    case FloorPlanAuditStatus.Completed:
                    case FloorPlanAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case FloorPlanAuditStatus.Completed:
                return next == FloorPlanAuditStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FloorPlanAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Discount - Ceiling, 2);
    }

    public FloorPlanAudit Copy()
    {
        return new FloorPlanAudit(Code, Title, Currency, Ceiling, Region, Discount, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
