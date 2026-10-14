namespace Acme.Workforce.Contractors.Timesheet.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet score is in its life cycle.</summary>
public enum TimesheetScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A timesheet score. Its code is unique within a tenant.</summary>
public sealed class TimesheetScore(string code, string title, double rating, string skill, List<string> tags, double ratio, int graceDays, TimesheetScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rating { get; set; } = rating;
    public string Skill { get; set; } = skill;
    public List<string> Tags { get; set; } = tags;
    public double Ratio { get; set; } = ratio;
    public int GraceDays { get; set; } = graceDays;
    public TimesheetScoreStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet score in status draft.</summary>
    public static TimesheetScore NewTimesheetScore(string code, string title)
    {
        return new TimesheetScore(code, title, 0.0, "", new List<string>(), 0.0, 0, TimesheetScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetScoreStatus next)
    {
        switch (Status)
        {
            case TimesheetScoreStatus.Draft:
                return next == TimesheetScoreStatus.Pending;
            case TimesheetScoreStatus.Pending:
                return next == TimesheetScoreStatus.Suspended || next == TimesheetScoreStatus.Closed;
            case TimesheetScoreStatus.Active:
                switch (next)
                {
                    case TimesheetScoreStatus.Suspended:
                    case TimesheetScoreStatus.Closed:
                        return true;
                }
                return false;
            case TimesheetScoreStatus.Suspended:
                return next == TimesheetScoreStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TimesheetScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet score {Code} cannot move from {Status} to {next}");
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

    public double NetExposure()
    {
        return Math.Round(Rating + (double)GraceDays, 2);
    }

    public TimesheetScore Copy()
    {
        return new TimesheetScore(Code, Title, Rating, Skill, Tags, Ratio, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
