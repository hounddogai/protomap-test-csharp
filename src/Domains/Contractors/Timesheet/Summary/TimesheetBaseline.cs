namespace Acme.Workforce.Contractors.Timesheet.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet baseline is in its life cycle.</summary>
public enum TimesheetBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A timesheet baseline. Its code is unique within a tenant.</summary>
public sealed class TimesheetBaseline(string code, string title, double margin, int batchSize, string name, string unit, int endsAt, TimesheetBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int BatchSize { get; set; } = batchSize;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public int EndsAt { get; set; } = endsAt;
    public TimesheetBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet baseline in status proposed.</summary>
    public static TimesheetBaseline NewTimesheetBaseline(string code, string title)
    {
        return new TimesheetBaseline(code, title, 0.0, 0, "", "", 0, TimesheetBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetBaselineStatus next)
    {
        switch (Status)
        {
            case TimesheetBaselineStatus.Proposed:
                switch (next)
                {
                    case TimesheetBaselineStatus.Approved:
                    case TimesheetBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case TimesheetBaselineStatus.Approved:
                return next == TimesheetBaselineStatus.Withdrawn;
            case TimesheetBaselineStatus.Rejected:
                return next == TimesheetBaselineStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TimesheetBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetTotal()
    {
        return Math.Round((double)BatchSize + Margin, 2);
    }

    public TimesheetBaseline Copy()
    {
        return new TimesheetBaseline(Code, Title, Margin, BatchSize, Name, Unit, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
