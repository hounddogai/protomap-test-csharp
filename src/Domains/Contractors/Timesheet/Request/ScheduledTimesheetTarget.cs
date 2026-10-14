namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled timesheet target is in its life cycle.</summary>
public enum ScheduledTimesheetTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled timesheet target. Its code is unique within a tenant.</summary>
public sealed class ScheduledTimesheetTarget(string code, string title, string reviewer, string department, int tenureMonths, ScheduledTimesheetTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Department { get; set; } = department;
    public int TenureMonths { get; set; } = tenureMonths;
    public ScheduledTimesheetTargetStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled timesheet target in status proposed.</summary>
    public static ScheduledTimesheetTarget NewScheduledTimesheetTarget(string code, string title)
    {
        return new ScheduledTimesheetTarget(code, title, "", "", 0, ScheduledTimesheetTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTimesheetTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTimesheetTargetStatus next)
    {
        switch (Status)
        {
            case ScheduledTimesheetTargetStatus.Proposed:
                return next == ScheduledTimesheetTargetStatus.Approved || next == ScheduledTimesheetTargetStatus.Rejected;
            case ScheduledTimesheetTargetStatus.Approved:
                switch (next)
                {
                    case ScheduledTimesheetTargetStatus.Rejected:
                    case ScheduledTimesheetTargetStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledTimesheetTargetStatus.Rejected:
                switch (next)
                {
                    case ScheduledTimesheetTargetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledTimesheetTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled timesheet target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled timesheet target"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public ScheduledTimesheetTarget Copy()
    {
        return new ScheduledTimesheetTarget(Code, Title, Reviewer, Department, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
