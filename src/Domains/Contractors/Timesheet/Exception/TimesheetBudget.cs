namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet budget is in its life cycle.</summary>
public enum TimesheetBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A timesheet budget as the timesheet team keeps it.</summary>
public sealed class TimesheetBudget(string code, string name, string country, string segment, string source, double score, TimesheetBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public double Score { get; set; } = score;
    public TimesheetBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet budget in status draft.</summary>
    public static TimesheetBudget NewTimesheetBudget(string code, string name)
    {
        return new TimesheetBudget(code, name, "", "", "", 0.0, TimesheetBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetBudgetStatus next)
    {
        switch (Status)
        {
            case TimesheetBudgetStatus.Draft:
                return next == TimesheetBudgetStatus.Closed;
            case TimesheetBudgetStatus.Pending:
                return next == TimesheetBudgetStatus.Suspended || next == TimesheetBudgetStatus.Closed;
            case TimesheetBudgetStatus.Active:
                return next == TimesheetBudgetStatus.Closed;
            case TimesheetBudgetStatus.Suspended:
                switch (next)
                {
                    case TimesheetBudgetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public TimesheetBudget Copy()
    {
        return new TimesheetBudget(Code, Name, Country, Segment, Source, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
