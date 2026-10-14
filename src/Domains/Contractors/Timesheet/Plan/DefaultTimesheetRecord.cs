namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default timesheet record is in its life cycle.</summary>
public enum DefaultTimesheetRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One default timesheet record, with what the planners need to act on it.</summary>
public sealed class DefaultTimesheetRecord(string code, string name, string skill, double minimum, string category, int graceDays, bool visible, string notes, string source, DefaultTimesheetRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Skill { get; set; } = skill;
    public double Minimum { get; set; } = minimum;
    public string Category { get; set; } = category;
    public int GraceDays { get; set; } = graceDays;
    public bool Visible { get; set; } = visible;
    public string Notes { get; set; } = notes;
    public string Source { get; set; } = source;
    public DefaultTimesheetRecordStatus Status { get; set; } = status;

    /// <summary>Starts a default timesheet record in status open.</summary>
    public static DefaultTimesheetRecord NewDefaultTimesheetRecord(string code, string name)
    {
        return new DefaultTimesheetRecord(
            code,
            name,
            "",
            0.0,
            "",
            0,
            false,
            "",
            "",
            DefaultTimesheetRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultTimesheetRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultTimesheetRecordStatus next)
    {
        switch (Status)
        {
            case DefaultTimesheetRecordStatus.Open:
                return next == DefaultTimesheetRecordStatus.InReview || next == DefaultTimesheetRecordStatus.Archived;
            case DefaultTimesheetRecordStatus.InReview:
                switch (next)
                {
                    case DefaultTimesheetRecordStatus.Resolved:
                    case DefaultTimesheetRecordStatus.Archived:
                        return true;
                }
                return false;
            case DefaultTimesheetRecordStatus.Resolved:
                return next == DefaultTimesheetRecordStatus.Archived;
            case DefaultTimesheetRecordStatus.Reopened:
                return next == DefaultTimesheetRecordStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DefaultTimesheetRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default timesheet record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Minimum * (double)GraceDays, 2);
    }

    public DefaultTimesheetRecord Copy()
    {
        return new DefaultTimesheetRecord(
            Code,
            Name,
            Skill,
            Minimum,
            Category,
            GraceDays,
            Visible,
            Notes,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
