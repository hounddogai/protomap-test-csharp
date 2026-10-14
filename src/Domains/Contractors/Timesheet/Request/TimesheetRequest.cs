namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet request is in its life cycle.</summary>
public enum TimesheetRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a timesheet request and its current status.</summary>
public sealed class TimesheetRequest(string code, string name, int windowDays, string source, string salaryBand, string phone, double hours, double minimum, string contactEmail, int priority, string externalRef, TimesheetRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Source { get; set; } = source;
    public string SalaryBand { get; set; } = salaryBand;
    public string Phone { get; set; } = phone;
    public double Hours { get; set; } = hours;
    public double Minimum { get; set; } = minimum;
    public string ContactEmail { get; set; } = contactEmail;
    public int Priority { get; set; } = priority;
    public string ExternalRef { get; set; } = externalRef;
    public TimesheetRequestStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet request in status draft.</summary>
    public static TimesheetRequest NewTimesheetRequest(string code, string name)
    {
        return new TimesheetRequest(code, name, 0, "", "", "", 0.0, 0.0, "", 0, "", TimesheetRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetRequestStatus next)
    {
        switch (Status)
        {
            case TimesheetRequestStatus.Draft:
                switch (next)
                {
                    case TimesheetRequestStatus.Active:
                        return true;
                }
                return false;
            case TimesheetRequestStatus.Pending:
                switch (next)
                {
                    case TimesheetRequestStatus.Closed:
                        return true;
                }
                return false;
            case TimesheetRequestStatus.Active:
                switch (next)
                {
                    case TimesheetRequestStatus.Suspended:
                    case TimesheetRequestStatus.Closed:
                        return true;
                }
                return false;
            case TimesheetRequestStatus.Suspended:
                return next == TimesheetRequestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TimesheetRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Hours - (double)Priority, 2);
    }

    public TimesheetRequest Copy()
    {
        return new TimesheetRequest(
            Code,
            Name,
            WindowDays,
            Source,
            SalaryBand,
            Phone,
            Hours,
            Minimum,
            ContactEmail,
            Priority,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
