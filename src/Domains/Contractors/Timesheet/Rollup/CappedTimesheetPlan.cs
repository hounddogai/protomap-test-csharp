namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped timesheet plan is in its life cycle.</summary>
public enum CappedTimesheetPlanStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One capped timesheet plan, with what the planners need to act on it.</summary>
public sealed class CappedTimesheetPlan(string code, string name, int attempts, double minimum, int capacity, double weight, CappedTimesheetPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public double Minimum { get; set; } = minimum;
    public int Capacity { get; set; } = capacity;
    public double Weight { get; set; } = weight;
    public CappedTimesheetPlanStatus Status { get; set; } = status;

    /// <summary>Starts a capped timesheet plan in status proposed.</summary>
    public static CappedTimesheetPlan NewCappedTimesheetPlan(string code, string name)
    {
        return new CappedTimesheetPlan(code, name, 0, 0.0, 0, 0.0, CappedTimesheetPlanStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedTimesheetPlanStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedTimesheetPlanStatus next)
    {
        switch (Status)
        {
            case CappedTimesheetPlanStatus.Proposed:
                switch (next)
                {
                    case CappedTimesheetPlanStatus.Rejected:
                    case CappedTimesheetPlanStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedTimesheetPlanStatus.Approved:
                switch (next)
                {
                    case CappedTimesheetPlanStatus.Withdrawn:
                        return true;
                }
                return false;
            case CappedTimesheetPlanStatus.Rejected:
                return next == CappedTimesheetPlanStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(CappedTimesheetPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped timesheet plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped timesheet plan"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Capacity * Minimum, 2);
    }

    public CappedTimesheetPlan Copy()
    {
        return new CappedTimesheetPlan(Code, Name, Attempts, Minimum, Capacity, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
