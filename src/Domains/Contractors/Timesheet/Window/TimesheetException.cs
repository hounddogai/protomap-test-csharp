namespace Acme.Workforce.Contractors.Timesheet.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet exception is in its life cycle.</summary>
public enum TimesheetExceptionStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A timesheet exception. Its code is unique within a tenant.</summary>
public sealed class TimesheetException(string code, string title, string currency, double minimum, string team, List<string> tags, int attempts, double weight, string jobCode, string phone, int quantity, TimesheetExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Minimum { get; set; } = minimum;
    public string Team { get; set; } = team;
    public List<string> Tags { get; set; } = tags;
    public int Attempts { get; set; } = attempts;
    public double Weight { get; set; } = weight;
    public string JobCode { get; set; } = jobCode;
    public string Phone { get; set; } = phone;
    public int Quantity { get; set; } = quantity;
    public TimesheetExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet exception in status planned.</summary>
    public static TimesheetException NewTimesheetException(string code, string title)
    {
        return new TimesheetException(
            code,
            title,
            "",
            0.0,
            "",
            new List<string>(),
            0,
            0.0,
            "",
            "",
            0,
            TimesheetExceptionStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetExceptionStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetExceptionStatus next)
    {
        switch (Status)
        {
            case TimesheetExceptionStatus.Planned:
                return next == TimesheetExceptionStatus.Scheduled || next == TimesheetExceptionStatus.InProgress;
            case TimesheetExceptionStatus.Scheduled:
                return next == TimesheetExceptionStatus.Cancelled;
            case TimesheetExceptionStatus.InProgress:
                switch (next)
                {
                    case TimesheetExceptionStatus.Completed:
                    case TimesheetExceptionStatus.Cancelled:
                        return true;
                }
                return false;
            case TimesheetExceptionStatus.Completed:
                switch (next)
                {
                    case TimesheetExceptionStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedLoad()
    {
        return Math.Round(Weight - Minimum, 2);
    }

    public TimesheetException Copy()
    {
        return new TimesheetException(
            Code,
            Title,
            Currency,
            Minimum,
            Team,
            Tags,
            Attempts,
            Weight,
            JobCode,
            Phone,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
