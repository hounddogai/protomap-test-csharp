namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient desk adjustment is in its life cycle.</summary>
public enum LenientDeskAdjustmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient desk adjustment as the desk team keeps it.</summary>
public sealed class LenientDeskAdjustment(string code, string label, string name, string description, string externalRef, string managerId, bool archived, double floorValue, LenientDeskAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public string ExternalRef { get; set; } = externalRef;
    public string ManagerId { get; set; } = managerId;
    public bool Archived { get; set; } = archived;
    public double FloorValue { get; set; } = floorValue;
    public LenientDeskAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a lenient desk adjustment in status planned.</summary>
    public static LenientDeskAdjustment NewLenientDeskAdjustment(string code, string label)
    {
        return new LenientDeskAdjustment(
            code,
            label,
            "",
            "",
            "",
            "",
            false,
            0.0,
            LenientDeskAdjustmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeskAdjustmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeskAdjustmentStatus next)
    {
        switch (Status)
        {
            case LenientDeskAdjustmentStatus.Planned:
                return next == LenientDeskAdjustmentStatus.InProgress || next == LenientDeskAdjustmentStatus.Cancelled;
            case LenientDeskAdjustmentStatus.Scheduled:
                return next == LenientDeskAdjustmentStatus.Completed || next == LenientDeskAdjustmentStatus.Cancelled;
            case LenientDeskAdjustmentStatus.InProgress:
                return next == LenientDeskAdjustmentStatus.Completed || next == LenientDeskAdjustmentStatus.Cancelled;
            case LenientDeskAdjustmentStatus.Completed:
                return next == LenientDeskAdjustmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientDeskAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient desk adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public LenientDeskAdjustment Copy()
    {
        return new LenientDeskAdjustment(
            Code,
            Label,
            Name,
            Description,
            ExternalRef,
            ManagerId,
            Archived,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
