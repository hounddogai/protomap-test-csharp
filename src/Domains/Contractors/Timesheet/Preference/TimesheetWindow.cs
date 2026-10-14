namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet window is in its life cycle.</summary>
public enum TimesheetWindowStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A timesheet window. Its code is unique within a tenant.</summary>
public sealed class TimesheetWindow(string code, string title, string label, double ratio, string timezone, string location, List<string> tags, string segment, int version, double score, TimesheetWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public string Timezone { get; set; } = timezone;
    public string Location { get; set; } = location;
    public List<string> Tags { get; set; } = tags;
    public string Segment { get; set; } = segment;
    public int Version { get; set; } = version;
    public double Score { get; set; } = score;
    public TimesheetWindowStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet window in status open.</summary>
    public static TimesheetWindow NewTimesheetWindow(string code, string title)
    {
        return new TimesheetWindow(
            code,
            title,
            "",
            0.0,
            "",
            "",
            new List<string>(),
            "",
            0,
            0.0,
            TimesheetWindowStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetWindowStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetWindowStatus next)
    {
        switch (Status)
        {
            case TimesheetWindowStatus.Open:
                switch (next)
                {
                    case TimesheetWindowStatus.Resolved:
                    case TimesheetWindowStatus.Reopened:
                        return true;
                }
                return false;
            case TimesheetWindowStatus.InReview:
                switch (next)
                {
                    case TimesheetWindowStatus.Reopened:
                        return true;
                }
                return false;
            case TimesheetWindowStatus.Resolved:
                return next == TimesheetWindowStatus.Archived;
            case TimesheetWindowStatus.Reopened:
                switch (next)
                {
                    case TimesheetWindowStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
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

    public double ProjectedTotal()
    {
        return Math.Round(Score - Ratio, 2);
    }

    public TimesheetWindow Copy()
    {
        return new TimesheetWindow(
            Code,
            Title,
            Label,
            Ratio,
            Timezone,
            Location,
            Tags,
            Segment,
            Version,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
