namespace Acme.Workforce.Contractors.Timesheet.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet summary is in its life cycle.</summary>
public enum TimesheetSummaryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One timesheet summary, with what the planners need to act on it.</summary>
public sealed class TimesheetSummary(string code, string name, int graceDays, string phone, double ceiling, bool visible, int durationMinutes, double ratio, string grade, string ownerId, TimesheetSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public string Phone { get; set; } = phone;
    public double Ceiling { get; set; } = ceiling;
    public bool Visible { get; set; } = visible;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Ratio { get; set; } = ratio;
    public string Grade { get; set; } = grade;
    public string OwnerId { get; set; } = ownerId;
    public TimesheetSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet summary in status created.</summary>
    public static TimesheetSummary NewTimesheetSummary(string code, string name)
    {
        return new TimesheetSummary(code, name, 0, "", 0.0, false, 0, 0.0, "", "", TimesheetSummaryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetSummaryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetSummaryStatus next)
    {
        switch (Status)
        {
            case TimesheetSummaryStatus.Created:
                return next == TimesheetSummaryStatus.Validated || next == TimesheetSummaryStatus.Published;
            case TimesheetSummaryStatus.Validated:
                switch (next)
                {
                    case TimesheetSummaryStatus.Published:
                    case TimesheetSummaryStatus.Retired:
                        return true;
                }
                return false;
            case TimesheetSummaryStatus.Published:
                switch (next)
                {
                    case TimesheetSummaryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"timesheet summary"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Ratio * (double)GraceDays, 2);
    }

    public TimesheetSummary Copy()
    {
        return new TimesheetSummary(
            Code,
            Name,
            GraceDays,
            Phone,
            Ceiling,
            Visible,
            DurationMinutes,
            Ratio,
            Grade,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
