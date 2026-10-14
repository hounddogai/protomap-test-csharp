namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled statement of work segment is in its life cycle.</summary>
public enum ScheduledStatementOfWorkSegmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a scheduled statement of work segment and its current status.</summary>
public sealed class ScheduledStatementOfWorkSegment(string code, string name, string country, int durationMinutes, double score, string employeeId, ScheduledStatementOfWorkSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Score { get; set; } = score;
    public string EmployeeId { get; set; } = employeeId;
    public ScheduledStatementOfWorkSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled statement of work segment in status open.</summary>
    public static ScheduledStatementOfWorkSegment NewScheduledStatementOfWorkSegment(string code, string name)
    {
        return new ScheduledStatementOfWorkSegment(
            code,
            name,
            "",
            0,
            0.0,
            "",
            ScheduledStatementOfWorkSegmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledStatementOfWorkSegmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledStatementOfWorkSegmentStatus next)
    {
        switch (Status)
        {
            case ScheduledStatementOfWorkSegmentStatus.Open:
                return next == ScheduledStatementOfWorkSegmentStatus.InReview;
            case ScheduledStatementOfWorkSegmentStatus.InReview:
                switch (next)
                {
                    case ScheduledStatementOfWorkSegmentStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledStatementOfWorkSegmentStatus.Resolved:
                switch (next)
                {
                    case ScheduledStatementOfWorkSegmentStatus.Archived:
                        return true;
                }
                return false;
            case ScheduledStatementOfWorkSegmentStatus.Reopened:
                return next == ScheduledStatementOfWorkSegmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledStatementOfWorkSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled statement of work segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)DurationMinutes + Score, 2);
    }

    public ScheduledStatementOfWorkSegment Copy()
    {
        return new ScheduledStatementOfWorkSegment(Code, Name, Country, DurationMinutes, Score, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
