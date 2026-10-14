namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan exception is in its life cycle.</summary>
public enum FloorPlanExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A floor plan exception as the floor plan team keeps it.</summary>
public sealed class FloorPlanException(string code, string label, int graceDays, int leaveDays, bool approved, int limit, string skill, FloorPlanExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Approved { get; set; } = approved;
    public int Limit { get; set; } = limit;
    public string Skill { get; set; } = skill;
    public FloorPlanExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan exception in status requested.</summary>
    public static FloorPlanException NewFloorPlanException(string code, string label)
    {
        return new FloorPlanException(code, label, 0, 0, false, 0, "", FloorPlanExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanExceptionStatus next)
    {
        switch (Status)
        {
            case FloorPlanExceptionStatus.Requested:
                switch (next)
                {
                    case FloorPlanExceptionStatus.Revoked:
                        return true;
                }
                return false;
            case FloorPlanExceptionStatus.Granted:
                return next == FloorPlanExceptionStatus.Revoked || next == FloorPlanExceptionStatus.Expired;
            case FloorPlanExceptionStatus.Revoked:
                return next == FloorPlanExceptionStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FloorPlanExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"floor plan exception"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)GraceDays - (double)Limit, 2);
    }

    public FloorPlanException Copy()
    {
        return new FloorPlanException(Code, Label, GraceDays, LeaveDays, Approved, Limit, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
