namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet milestone is in its life cycle.</summary>
public enum TimesheetMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One timesheet milestone, with what the planners need to act on it.</summary>
public sealed class TimesheetMilestone(string code, string name, bool active, int createdAt, string reviewer, TimesheetMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public int CreatedAt { get; set; } = createdAt;
    public string Reviewer { get; set; } = reviewer;
    public TimesheetMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet milestone in status proposed.</summary>
    public static TimesheetMilestone NewTimesheetMilestone(string code, string name)
    {
        return new TimesheetMilestone(code, name, false, 0, "", TimesheetMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetMilestoneStatus next)
    {
        switch (Status)
        {
            case TimesheetMilestoneStatus.Proposed:
                switch (next)
                {
                    case TimesheetMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case TimesheetMilestoneStatus.Approved:
                switch (next)
                {
                    case TimesheetMilestoneStatus.Rejected:
                        return true;
                }
                return false;
            case TimesheetMilestoneStatus.Rejected:
                switch (next)
                {
                    case TimesheetMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public TimesheetMilestone Copy()
    {
        return new TimesheetMilestone(Code, Name, Active, CreatedAt, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
