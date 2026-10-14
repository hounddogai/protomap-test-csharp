namespace Acme.Workforce.Contractors.Timesheet.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet record is in its life cycle.</summary>
public enum TimesheetRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a timesheet record and its current status.</summary>
public sealed class TimesheetRecord(string code, string name, bool archived, string location, int attempts, double minimum, int expiresAt, string summary, TimesheetRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public string Location { get; set; } = location;
    public int Attempts { get; set; } = attempts;
    public double Minimum { get; set; } = minimum;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Summary { get; set; } = summary;
    public TimesheetRecordStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet record in status draft.</summary>
    public static TimesheetRecord NewTimesheetRecord(string code, string name)
    {
        return new TimesheetRecord(code, name, false, "", 0, 0.0, 0, "", TimesheetRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetRecordStatus next)
    {
        switch (Status)
        {
            case TimesheetRecordStatus.Draft:
                return next == TimesheetRecordStatus.Active;
            case TimesheetRecordStatus.Pending:
                switch (next)
                {
                    case TimesheetRecordStatus.Active:
                    case TimesheetRecordStatus.Suspended:
                        return true;
                }
                return false;
            case TimesheetRecordStatus.Active:
                return next == TimesheetRecordStatus.Suspended;
            case TimesheetRecordStatus.Suspended:
                return next == TimesheetRecordStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TimesheetRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Attempts + Minimum, 2);
    }

    public TimesheetRecord Copy()
    {
        return new TimesheetRecord(Code, Name, Archived, Location, Attempts, Minimum, ExpiresAt, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
