namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet rollup is in its life cycle.</summary>
public enum TimesheetRollupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a timesheet rollup and its current status.</summary>
public sealed class TimesheetRollup(string code, string label, bool active, string costCenter, double maximum, string department, TimesheetRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string CostCenter { get; set; } = costCenter;
    public double Maximum { get; set; } = maximum;
    public string Department { get; set; } = department;
    public TimesheetRollupStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet rollup in status requested.</summary>
    public static TimesheetRollup NewTimesheetRollup(string code, string label)
    {
        return new TimesheetRollup(code, label, false, "", 0.0, "", TimesheetRollupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetRollupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetRollupStatus next)
    {
        switch (Status)
        {
            case TimesheetRollupStatus.Requested:
                return next == TimesheetRollupStatus.Revoked || next == TimesheetRollupStatus.Expired;
            case TimesheetRollupStatus.Granted:
                return next == TimesheetRollupStatus.Revoked || next == TimesheetRollupStatus.Expired;
            case TimesheetRollupStatus.Revoked:
                switch (next)
                {
                    case TimesheetRollupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public TimesheetRollup Copy()
    {
        return new TimesheetRollup(Code, Label, Active, CostCenter, Maximum, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
