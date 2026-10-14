namespace Acme.Workforce.Contractors.Timesheet.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet segment is in its life cycle.</summary>
public enum TimesheetSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One timesheet segment, with what the planners need to act on it.</summary>
public sealed class TimesheetSegment(string code, string name, int revision, int priority, string skill, string summary, TimesheetSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public int Priority { get; set; } = priority;
    public string Skill { get; set; } = skill;
    public string Summary { get; set; } = summary;
    public TimesheetSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet segment in status draft.</summary>
    public static TimesheetSegment NewTimesheetSegment(string code, string name)
    {
        return new TimesheetSegment(code, name, 0, 0, "", "", TimesheetSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetSegmentStatus next)
    {
        switch (Status)
        {
            case TimesheetSegmentStatus.Draft:
                switch (next)
                {
                    case TimesheetSegmentStatus.Active:
                    case TimesheetSegmentStatus.Closed:
                        return true;
                }
                return false;
            case TimesheetSegmentStatus.Pending:
                return next == TimesheetSegmentStatus.Suspended;
            case TimesheetSegmentStatus.Active:
                return next == TimesheetSegmentStatus.Suspended;
            case TimesheetSegmentStatus.Suspended:
                return next == TimesheetSegmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TimesheetSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Revision - (double)Priority, 2);
    }

    public TimesheetSegment Copy()
    {
        return new TimesheetSegment(Code, Name, Revision, Priority, Skill, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
