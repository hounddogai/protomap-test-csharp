namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet bundle is in its life cycle.</summary>
public enum TimesheetBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A timesheet bundle. Its code is unique within a tenant.</summary>
public sealed class TimesheetBundle(string code, string title, string category, int graceDays, string jobCode, string costCenter, int startsAt, TimesheetBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public int GraceDays { get; set; } = graceDays;
    public string JobCode { get; set; } = jobCode;
    public string CostCenter { get; set; } = costCenter;
    public int StartsAt { get; set; } = startsAt;
    public TimesheetBundleStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet bundle in status idle.</summary>
    public static TimesheetBundle NewTimesheetBundle(string code, string title)
    {
        return new TimesheetBundle(code, title, "", 0, "", "", 0, TimesheetBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetBundleStatus next)
    {
        switch (Status)
        {
            case TimesheetBundleStatus.Idle:
                return next == TimesheetBundleStatus.Stopped;
            case TimesheetBundleStatus.Warming:
                switch (next)
                {
                    case TimesheetBundleStatus.Ready:
                    case TimesheetBundleStatus.Stopped:
                        return true;
                }
                return false;
            case TimesheetBundleStatus.Ready:
                return next == TimesheetBundleStatus.Draining || next == TimesheetBundleStatus.Stopped;
            case TimesheetBundleStatus.Draining:
                return next == TimesheetBundleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TimesheetBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"timesheet bundle"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)GraceDays + (double)StartsAt, 2);
    }

    public TimesheetBundle Copy()
    {
        return new TimesheetBundle(Code, Title, Category, GraceDays, JobCode, CostCenter, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
