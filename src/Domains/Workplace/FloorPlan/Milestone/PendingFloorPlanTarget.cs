namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending floor plan target is in its life cycle.</summary>
public enum PendingFloorPlanTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending floor plan target. Its code is unique within a tenant.</summary>
public sealed class PendingFloorPlanTarget(string code, string label, string jobCode, int windowDays, int quantity, int capacity, double rate, string description, double amount, PendingFloorPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int WindowDays { get; set; } = windowDays;
    public int Quantity { get; set; } = quantity;
    public int Capacity { get; set; } = capacity;
    public double Rate { get; set; } = rate;
    public string Description { get; set; } = description;
    public double Amount { get; set; } = amount;
    public PendingFloorPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pending floor plan target in status proposed.</summary>
    public static PendingFloorPlanTarget NewPendingFloorPlanTarget(string code, string label)
    {
        return new PendingFloorPlanTarget(
            code,
            label,
            "",
            0,
            0,
            0,
            0.0,
            "",
            0.0,
            PendingFloorPlanTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingFloorPlanTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingFloorPlanTargetStatus next)
    {
        switch (Status)
        {
            case PendingFloorPlanTargetStatus.Proposed:
                switch (next)
                {
                    case PendingFloorPlanTargetStatus.Rejected:
                        return true;
                }
                return false;
            case PendingFloorPlanTargetStatus.Approved:
                return next == PendingFloorPlanTargetStatus.Rejected;
            case PendingFloorPlanTargetStatus.Rejected:
                return next == PendingFloorPlanTargetStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PendingFloorPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending floor plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending floor plan target"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Rate * (double)Quantity, 2);
    }

    public PendingFloorPlanTarget Copy()
    {
        return new PendingFloorPlanTarget(
            Code,
            Label,
            JobCode,
            WindowDays,
            Quantity,
            Capacity,
            Rate,
            Description,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
