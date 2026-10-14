namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled leave request mapping is in its life cycle.</summary>
public enum ScheduledLeaveRequestMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a scheduled leave request mapping and its current status.</summary>
public sealed class ScheduledLeaveRequestMapping(string code, string title, int priority, string segment, int expiresAt, double threshold, ScheduledLeaveRequestMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public string Segment { get; set; } = segment;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Threshold { get; set; } = threshold;
    public ScheduledLeaveRequestMappingStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled leave request mapping in status open.</summary>
    public static ScheduledLeaveRequestMapping NewScheduledLeaveRequestMapping(string code, string title)
    {
        return new ScheduledLeaveRequestMapping(code, title, 0, "", 0, 0.0, ScheduledLeaveRequestMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledLeaveRequestMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledLeaveRequestMappingStatus next)
    {
        switch (Status)
        {
            case ScheduledLeaveRequestMappingStatus.Open:
                switch (next)
                {
                    case ScheduledLeaveRequestMappingStatus.Reopened:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestMappingStatus.InReview:
                return next == ScheduledLeaveRequestMappingStatus.Resolved || next == ScheduledLeaveRequestMappingStatus.Reopened;
            case ScheduledLeaveRequestMappingStatus.Resolved:
                return next == ScheduledLeaveRequestMappingStatus.Reopened || next == ScheduledLeaveRequestMappingStatus.Archived;
            case ScheduledLeaveRequestMappingStatus.Reopened:
                return next == ScheduledLeaveRequestMappingStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ScheduledLeaveRequestMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled leave request mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)ExpiresAt * Threshold, 2);
    }

    public ScheduledLeaveRequestMapping Copy()
    {
        return new ScheduledLeaveRequestMapping(Code, Title, Priority, Segment, ExpiresAt, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
