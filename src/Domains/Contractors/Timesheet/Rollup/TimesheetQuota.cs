namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet quota is in its life cycle.</summary>
public enum TimesheetQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One timesheet quota, with what the planners need to act on it.</summary>
public sealed class TimesheetQuota(string code, string label, string badge, string skill, bool active, double hours, TimesheetQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Skill { get; set; } = skill;
    public bool Active { get; set; } = active;
    public double Hours { get; set; } = hours;
    public TimesheetQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet quota in status new.</summary>
    public static TimesheetQuota NewTimesheetQuota(string code, string label)
    {
        return new TimesheetQuota(code, label, "", "", false, 0.0, TimesheetQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetQuotaStatus next)
    {
        switch (Status)
        {
            case TimesheetQuotaStatus.New:
                return next == TimesheetQuotaStatus.Running || next == TimesheetQuotaStatus.Done;
            case TimesheetQuotaStatus.Queued:
                return next == TimesheetQuotaStatus.Running;
            case TimesheetQuotaStatus.Running:
                switch (next)
                {
                    case TimesheetQuotaStatus.Done:
                    case TimesheetQuotaStatus.Failed:
                        return true;
                }
                return false;
            case TimesheetQuotaStatus.Done:
                switch (next)
                {
                    case TimesheetQuotaStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet quota {Code} cannot move from {Status} to {next}");
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

    public TimesheetQuota Copy()
    {
        return new TimesheetQuota(Code, Label, Badge, Skill, Active, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
