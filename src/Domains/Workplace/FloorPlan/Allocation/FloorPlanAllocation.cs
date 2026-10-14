namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan allocation is in its life cycle.</summary>
public enum FloorPlanAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A floor plan allocation. Its code is unique within a tenant.</summary>
public sealed class FloorPlanAllocation(string code, string label, int expiresAt, string externalRef, bool approved, string grade, int durationMinutes, int revision, int windowDays, string costCenter, int attempts, FloorPlanAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ExternalRef { get; set; } = externalRef;
    public bool Approved { get; set; } = approved;
    public string Grade { get; set; } = grade;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Revision { get; set; } = revision;
    public int WindowDays { get; set; } = windowDays;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public FloorPlanAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan allocation in status planned.</summary>
    public static FloorPlanAllocation NewFloorPlanAllocation(string code, string label)
    {
        return new FloorPlanAllocation(
            code,
            label,
            0,
            "",
            false,
            "",
            0,
            0,
            0,
            "",
            0,
            FloorPlanAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanAllocationStatus next)
    {
        switch (Status)
        {
            case FloorPlanAllocationStatus.Planned:
                switch (next)
                {
                    case FloorPlanAllocationStatus.Scheduled:
                        return true;
                }
                return false;
            case FloorPlanAllocationStatus.Scheduled:
                return next == FloorPlanAllocationStatus.Cancelled;
            case FloorPlanAllocationStatus.InProgress:
                return next == FloorPlanAllocationStatus.Cancelled;
            case FloorPlanAllocationStatus.Completed:
                switch (next)
                {
                    case FloorPlanAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)Revision * (double)WindowDays, 2);
    }

    public FloorPlanAllocation Copy()
    {
        return new FloorPlanAllocation(
            Code,
            Label,
            ExpiresAt,
            ExternalRef,
            Approved,
            Grade,
            DurationMinutes,
            Revision,
            WindowDays,
            CostCenter,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
