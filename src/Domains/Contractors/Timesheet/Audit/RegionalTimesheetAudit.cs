namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional timesheet audit is in its life cycle.</summary>
public enum RegionalTimesheetAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a regional timesheet audit and its current status.</summary>
public sealed class RegionalTimesheetAudit(string code, string label, string title, double hours, string skill, bool approved, RegionalTimesheetAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Skill { get; set; } = skill;
    public bool Approved { get; set; } = approved;
    public RegionalTimesheetAuditStatus Status { get; set; } = status;

    /// <summary>Starts a regional timesheet audit in status idle.</summary>
    public static RegionalTimesheetAudit NewRegionalTimesheetAudit(string code, string label)
    {
        return new RegionalTimesheetAudit(code, label, "", 0.0, "", false, RegionalTimesheetAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTimesheetAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTimesheetAuditStatus next)
    {
        switch (Status)
        {
            case RegionalTimesheetAuditStatus.Idle:
                return next == RegionalTimesheetAuditStatus.Ready || next == RegionalTimesheetAuditStatus.Draining;
            case RegionalTimesheetAuditStatus.Warming:
                switch (next)
                {
                    case RegionalTimesheetAuditStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalTimesheetAuditStatus.Ready:
                switch (next)
                {
                    case RegionalTimesheetAuditStatus.Draining:
                    case RegionalTimesheetAuditStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalTimesheetAuditStatus.Draining:
                return next == RegionalTimesheetAuditStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RegionalTimesheetAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional timesheet audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public RegionalTimesheetAudit Copy()
    {
        return new RegionalTimesheetAudit(Code, Label, Title, Hours, Skill, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
