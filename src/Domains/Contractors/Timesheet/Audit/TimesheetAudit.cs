namespace Acme.Workforce.Contractors.Timesheet.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet audit is in its life cycle.</summary>
public enum TimesheetAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A timesheet audit. Its code is unique within a tenant.</summary>
public sealed class TimesheetAudit(string code, string name, string currency, string label, string badge, double rate, bool visible, int tenureMonths, TimesheetAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public double Rate { get; set; } = rate;
    public bool Visible { get; set; } = visible;
    public int TenureMonths { get; set; } = tenureMonths;
    public TimesheetAuditStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet audit in status idle.</summary>
    public static TimesheetAudit NewTimesheetAudit(string code, string name)
    {
        return new TimesheetAudit(code, name, "", "", "", 0.0, false, 0, TimesheetAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetAuditStatus next)
    {
        switch (Status)
        {
            case TimesheetAuditStatus.Idle:
                return next == TimesheetAuditStatus.Stopped;
            case TimesheetAuditStatus.Warming:
                return next == TimesheetAuditStatus.Ready || next == TimesheetAuditStatus.Stopped;
            case TimesheetAuditStatus.Ready:
                switch (next)
                {
                    case TimesheetAuditStatus.Stopped:
                        return true;
                }
                return false;
            case TimesheetAuditStatus.Draining:
                return next == TimesheetAuditStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TimesheetAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rate + (double)TenureMonths, 2);
    }

    public TimesheetAudit Copy()
    {
        return new TimesheetAudit(Code, Name, Currency, Label, Badge, Rate, Visible, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
