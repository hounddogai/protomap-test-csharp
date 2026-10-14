namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional timesheet window is in its life cycle.</summary>
public enum RegionalTimesheetWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One regional timesheet window, with what the planners need to act on it.</summary>
public sealed class RegionalTimesheetWindow(string code, string title, string summary, double minimum, int windowDays, int tenureMonths, RegionalTimesheetWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public double Minimum { get; set; } = minimum;
    public int WindowDays { get; set; } = windowDays;
    public int TenureMonths { get; set; } = tenureMonths;
    public RegionalTimesheetWindowStatus Status { get; set; } = status;

    /// <summary>Starts a regional timesheet window in status planned.</summary>
    public static RegionalTimesheetWindow NewRegionalTimesheetWindow(string code, string title)
    {
        return new RegionalTimesheetWindow(code, title, "", 0.0, 0, 0, RegionalTimesheetWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTimesheetWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTimesheetWindowStatus next)
    {
        switch (Status)
        {
            case RegionalTimesheetWindowStatus.Planned:
                return next == RegionalTimesheetWindowStatus.Completed || next == RegionalTimesheetWindowStatus.Cancelled;
            case RegionalTimesheetWindowStatus.Scheduled:
                switch (next)
                {
                    case RegionalTimesheetWindowStatus.InProgress:
                    case RegionalTimesheetWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalTimesheetWindowStatus.InProgress:
                return next == RegionalTimesheetWindowStatus.Completed || next == RegionalTimesheetWindowStatus.Cancelled;
            case RegionalTimesheetWindowStatus.Completed:
                return next == RegionalTimesheetWindowStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RegionalTimesheetWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional timesheet window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional timesheet window"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)WindowDays - (double)TenureMonths, 2);
    }

    public RegionalTimesheetWindow Copy()
    {
        return new RegionalTimesheetWindow(Code, Title, Summary, Minimum, WindowDays, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
