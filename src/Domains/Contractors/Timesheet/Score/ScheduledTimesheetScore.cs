namespace Acme.Workforce.Contractors.Timesheet.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled timesheet score is in its life cycle.</summary>
public enum ScheduledTimesheetScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scheduled timesheet score and its current status.</summary>
public sealed class ScheduledTimesheetScore(string code, string title, string segment, int attempts, bool approved, double maximum, string costCenter, ScheduledTimesheetScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public int Attempts { get; set; } = attempts;
    public bool Approved { get; set; } = approved;
    public double Maximum { get; set; } = maximum;
    public string CostCenter { get; set; } = costCenter;
    public ScheduledTimesheetScoreStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled timesheet score in status draft.</summary>
    public static ScheduledTimesheetScore NewScheduledTimesheetScore(string code, string title)
    {
        return new ScheduledTimesheetScore(code, title, "", 0, false, 0.0, "", ScheduledTimesheetScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTimesheetScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTimesheetScoreStatus next)
    {
        switch (Status)
        {
            case ScheduledTimesheetScoreStatus.Draft:
                switch (next)
                {
                    case ScheduledTimesheetScoreStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledTimesheetScoreStatus.Pending:
                switch (next)
                {
                    case ScheduledTimesheetScoreStatus.Suspended:
                    case ScheduledTimesheetScoreStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledTimesheetScoreStatus.Active:
                return next == ScheduledTimesheetScoreStatus.Suspended;
            case ScheduledTimesheetScoreStatus.Suspended:
                switch (next)
                {
                    case ScheduledTimesheetScoreStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledTimesheetScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled timesheet score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)Attempts * Maximum, 2);
    }

    public ScheduledTimesheetScore Copy()
    {
        return new ScheduledTimesheetScore(Code, Title, Segment, Attempts, Approved, Maximum, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
