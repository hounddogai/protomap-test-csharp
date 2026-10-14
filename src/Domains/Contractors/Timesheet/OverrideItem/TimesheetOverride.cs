namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet override is in its life cycle.</summary>
public enum TimesheetOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A timesheet override as the timesheet team keeps it.</summary>
public sealed class TimesheetOverride(string code, string label, int version, string region, int graceDays, string managerId, string name, string notes, TimesheetOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public int GraceDays { get; set; } = graceDays;
    public string ManagerId { get; set; } = managerId;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public TimesheetOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet override in status planned.</summary>
    public static TimesheetOverride NewTimesheetOverride(string code, string label)
    {
        return new TimesheetOverride(code, label, 0, "", 0, "", "", "", TimesheetOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetOverrideStatus next)
    {
        switch (Status)
        {
            case TimesheetOverrideStatus.Planned:
                return next == TimesheetOverrideStatus.Scheduled || next == TimesheetOverrideStatus.Cancelled;
            case TimesheetOverrideStatus.Scheduled:
                switch (next)
                {
                    case TimesheetOverrideStatus.InProgress:
                    case TimesheetOverrideStatus.Completed:
                        return true;
                }
                return false;
            case TimesheetOverrideStatus.InProgress:
                switch (next)
                {
                    case TimesheetOverrideStatus.Completed:
                    case TimesheetOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case TimesheetOverrideStatus.Completed:
                switch (next)
                {
                    case TimesheetOverrideStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Version + (double)GraceDays, 2);
    }

    public TimesheetOverride Copy()
    {
        return new TimesheetOverride(Code, Label, Version, Region, GraceDays, ManagerId, Name, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
