namespace Acme.Workforce.Contractors.Timesheet.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet metric is in its life cycle.</summary>
public enum TimesheetMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One timesheet metric, with what the planners need to act on it.</summary>
public sealed class TimesheetMetric(string code, string name, string managerId, double discount, bool visible, string contactEmail, double ratio, TimesheetMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ManagerId { get; set; } = managerId;
    public double Discount { get; set; } = discount;
    public bool Visible { get; set; } = visible;
    public string ContactEmail { get; set; } = contactEmail;
    public double Ratio { get; set; } = ratio;
    public TimesheetMetricStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet metric in status proposed.</summary>
    public static TimesheetMetric NewTimesheetMetric(string code, string name)
    {
        return new TimesheetMetric(code, name, "", 0.0, false, "", 0.0, TimesheetMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetMetricStatus next)
    {
        switch (Status)
        {
            case TimesheetMetricStatus.Proposed:
                return next == TimesheetMetricStatus.Rejected || next == TimesheetMetricStatus.Withdrawn;
            case TimesheetMetricStatus.Approved:
                switch (next)
                {
                    case TimesheetMetricStatus.Rejected:
                    case TimesheetMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case TimesheetMetricStatus.Rejected:
                switch (next)
                {
                    case TimesheetMetricStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Discount - Ratio, 2);
    }

    public TimesheetMetric Copy()
    {
        return new TimesheetMetric(Code, Name, ManagerId, Discount, Visible, ContactEmail, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
