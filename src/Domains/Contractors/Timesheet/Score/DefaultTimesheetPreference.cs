namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default timesheet preference is in its life cycle.</summary>
public enum DefaultTimesheetPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A default timesheet preference. Its code is unique within a tenant.</summary>
public sealed class DefaultTimesheetPreference(string code, string title, bool pinned, string segment, string name, double score, DefaultTimesheetPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Segment { get; set; } = segment;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public DefaultTimesheetPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a default timesheet preference in status open.</summary>
    public static DefaultTimesheetPreference NewDefaultTimesheetPreference(string code, string title)
    {
        return new DefaultTimesheetPreference(
            code,
            title,
            false,
            "",
            "",
            0.0,
            DefaultTimesheetPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultTimesheetPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultTimesheetPreferenceStatus next)
    {
        switch (Status)
        {
            case DefaultTimesheetPreferenceStatus.Open:
                switch (next)
                {
                    case DefaultTimesheetPreferenceStatus.Resolved:
                        return true;
                }
                return false;
            case DefaultTimesheetPreferenceStatus.InReview:
                return next == DefaultTimesheetPreferenceStatus.Resolved;
            case DefaultTimesheetPreferenceStatus.Resolved:
                return next == DefaultTimesheetPreferenceStatus.Archived;
            case DefaultTimesheetPreferenceStatus.Reopened:
                return next == DefaultTimesheetPreferenceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DefaultTimesheetPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default timesheet preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public DefaultTimesheetPreference Copy()
    {
        return new DefaultTimesheetPreference(Code, Title, Pinned, Segment, Name, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
