namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet plan is in its life cycle.</summary>
public enum TimesheetPlanStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A timesheet plan. Its code is unique within a tenant.</summary>
public sealed class TimesheetPlan(string code, string title, int attempts, double fte, int quantity, string name, string employeeId, string ownerId, TimesheetPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public double Fte { get; set; } = fte;
    public int Quantity { get; set; } = quantity;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string OwnerId { get; set; } = ownerId;
    public TimesheetPlanStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet plan in status proposed.</summary>
    public static TimesheetPlan NewTimesheetPlan(string code, string title)
    {
        return new TimesheetPlan(code, title, 0, 0.0, 0, "", "", "", TimesheetPlanStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetPlanStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetPlanStatus next)
    {
        switch (Status)
        {
            case TimesheetPlanStatus.Proposed:
                return next == TimesheetPlanStatus.Approved;
            case TimesheetPlanStatus.Approved:
                switch (next)
                {
                    case TimesheetPlanStatus.Withdrawn:
                        return true;
                }
                return false;
            case TimesheetPlanStatus.Rejected:
                switch (next)
                {
                    case TimesheetPlanStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Attempts - Fte, 2);
    }

    public TimesheetPlan Copy()
    {
        return new TimesheetPlan(Code, Title, Attempts, Fte, Quantity, Name, EmployeeId, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
