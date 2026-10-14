namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan cycle is in its life cycle.</summary>
public enum FloorPlanCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One floor plan cycle, with what the planners need to act on it.</summary>
public sealed class FloorPlanCycle(string code, string label, int capacity, bool approved, double floorValue, string unit, double ceiling, int revision, string skill, int leaveDays, FloorPlanCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public bool Approved { get; set; } = approved;
    public double FloorValue { get; set; } = floorValue;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public int Revision { get; set; } = revision;
    public string Skill { get; set; } = skill;
    public int LeaveDays { get; set; } = leaveDays;
    public FloorPlanCycleStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan cycle in status requested.</summary>
    public static FloorPlanCycle NewFloorPlanCycle(string code, string label)
    {
        return new FloorPlanCycle(code, label, 0, false, 0.0, "", 0.0, 0, "", 0, FloorPlanCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanCycleStatus next)
    {
        switch (Status)
        {
            case FloorPlanCycleStatus.Requested:
                return next == FloorPlanCycleStatus.Revoked;
            case FloorPlanCycleStatus.Granted:
                switch (next)
                {
                    case FloorPlanCycleStatus.Revoked:
                    case FloorPlanCycleStatus.Expired:
                        return true;
                }
                return false;
            case FloorPlanCycleStatus.Revoked:
                switch (next)
                {
                    case FloorPlanCycleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round((double)LeaveDays + (double)Revision, 2);
    }

    public FloorPlanCycle Copy()
    {
        return new FloorPlanCycle(
            Code,
            Label,
            Capacity,
            Approved,
            FloorValue,
            Unit,
            Ceiling,
            Revision,
            Skill,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
