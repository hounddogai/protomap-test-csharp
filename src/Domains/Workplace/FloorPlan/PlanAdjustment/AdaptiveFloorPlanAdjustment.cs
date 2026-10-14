namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive floor plan adjustment is in its life cycle.</summary>
public enum AdaptiveFloorPlanAdjustmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A adaptive floor plan adjustment. Its code is unique within a tenant.</summary>
public sealed class AdaptiveFloorPlanAdjustment(string code, string label, string name, int durationMinutes, string department, AdaptiveFloorPlanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Department { get; set; } = department;
    public AdaptiveFloorPlanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive floor plan adjustment in status created.</summary>
    public static AdaptiveFloorPlanAdjustment NewAdaptiveFloorPlanAdjustment(string code, string label)
    {
        return new AdaptiveFloorPlanAdjustment(code, label, "", 0, "", AdaptiveFloorPlanAdjustmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveFloorPlanAdjustmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveFloorPlanAdjustmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveFloorPlanAdjustmentStatus.Created:
                return next == AdaptiveFloorPlanAdjustmentStatus.Validated || next == AdaptiveFloorPlanAdjustmentStatus.Retired;
            case AdaptiveFloorPlanAdjustmentStatus.Validated:
                return next == AdaptiveFloorPlanAdjustmentStatus.Published || next == AdaptiveFloorPlanAdjustmentStatus.Retired;
            case AdaptiveFloorPlanAdjustmentStatus.Published:
                return next == AdaptiveFloorPlanAdjustmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveFloorPlanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive floor plan adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public AdaptiveFloorPlanAdjustment Copy()
    {
        return new AdaptiveFloorPlanAdjustment(Code, Label, Name, DurationMinutes, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
