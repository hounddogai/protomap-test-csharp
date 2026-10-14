namespace Acme.Workforce.Contractors.Timesheet.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet threshold is in its life cycle.</summary>
public enum TimesheetThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One timesheet threshold, with what the planners need to act on it.</summary>
public sealed class TimesheetThreshold(string code, string title, string source, string jobCode, int priority, double budget, double maximum, string unit, string category, TimesheetThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string JobCode { get; set; } = jobCode;
    public int Priority { get; set; } = priority;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public string Unit { get; set; } = unit;
    public string Category { get; set; } = category;
    public TimesheetThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet threshold in status new.</summary>
    public static TimesheetThreshold NewTimesheetThreshold(string code, string title)
    {
        return new TimesheetThreshold(code, title, "", "", 0, 0.0, 0.0, "", "", TimesheetThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetThresholdStatus next)
    {
        switch (Status)
        {
            case TimesheetThresholdStatus.New:
                return next == TimesheetThresholdStatus.Queued;
            case TimesheetThresholdStatus.Queued:
                switch (next)
                {
                    case TimesheetThresholdStatus.Done:
                        return true;
                }
                return false;
            case TimesheetThresholdStatus.Running:
                switch (next)
                {
                    case TimesheetThresholdStatus.Done:
                    case TimesheetThresholdStatus.Failed:
                        return true;
                }
                return false;
            case TimesheetThresholdStatus.Done:
                return next == TimesheetThresholdStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TimesheetThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Maximum * (double)Priority, 2);
    }

    public TimesheetThreshold Copy()
    {
        return new TimesheetThreshold(
            Code,
            Title,
            Source,
            JobCode,
            Priority,
            Budget,
            Maximum,
            Unit,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
