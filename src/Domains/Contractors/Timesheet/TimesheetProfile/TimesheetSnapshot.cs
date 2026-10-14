namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet snapshot is in its life cycle.</summary>
public enum TimesheetSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A timesheet snapshot. Its code is unique within a tenant.</summary>
public sealed class TimesheetSnapshot(string code, string title, string phone, int sequence, int windowDays, TimesheetSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public int Sequence { get; set; } = sequence;
    public int WindowDays { get; set; } = windowDays;
    public TimesheetSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet snapshot in status created.</summary>
    public static TimesheetSnapshot NewTimesheetSnapshot(string code, string title)
    {
        return new TimesheetSnapshot(code, title, "", 0, 0, TimesheetSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetSnapshotStatus next)
    {
        switch (Status)
        {
            case TimesheetSnapshotStatus.Created:
                switch (next)
                {
                    case TimesheetSnapshotStatus.Validated:
                    case TimesheetSnapshotStatus.Retired:
                        return true;
                }
                return false;
            case TimesheetSnapshotStatus.Validated:
                return next == TimesheetSnapshotStatus.Retired;
            case TimesheetSnapshotStatus.Published:
                switch (next)
                {
                    case TimesheetSnapshotStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"timesheet snapshot"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)WindowDays - (double)Sequence, 2);
    }

    public TimesheetSnapshot Copy()
    {
        return new TimesheetSnapshot(Code, Title, Phone, Sequence, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
