namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback return plan assignment is in its life cycle.</summary>
public enum FallbackReturnPlanAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback return plan assignment as the return plan team keeps it.</summary>
public sealed class FallbackReturnPlanAssignment(string code, string label, int quantity, int graceDays, string jobCode, FallbackReturnPlanAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public int GraceDays { get; set; } = graceDays;
    public string JobCode { get; set; } = jobCode;
    public FallbackReturnPlanAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback return plan assignment in status planned.</summary>
    public static FallbackReturnPlanAssignment NewFallbackReturnPlanAssignment(string code, string label)
    {
        return new FallbackReturnPlanAssignment(code, label, 0, 0, "", FallbackReturnPlanAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackReturnPlanAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackReturnPlanAssignmentStatus next)
    {
        switch (Status)
        {
            case FallbackReturnPlanAssignmentStatus.Planned:
                switch (next)
                {
                    case FallbackReturnPlanAssignmentStatus.Scheduled:
                        return true;
                }
                return false;
            case FallbackReturnPlanAssignmentStatus.Scheduled:
                return next == FallbackReturnPlanAssignmentStatus.InProgress || next == FallbackReturnPlanAssignmentStatus.Cancelled;
            case FallbackReturnPlanAssignmentStatus.InProgress:
                return next == FallbackReturnPlanAssignmentStatus.Completed || next == FallbackReturnPlanAssignmentStatus.Cancelled;
            case FallbackReturnPlanAssignmentStatus.Completed:
                switch (next)
                {
                    case FallbackReturnPlanAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackReturnPlanAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback return plan assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetTotal()
    {
        return Math.Round((double)GraceDays * (double)Quantity, 2);
    }

    public FallbackReturnPlanAssignment Copy()
    {
        return new FallbackReturnPlanAssignment(Code, Label, Quantity, GraceDays, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
