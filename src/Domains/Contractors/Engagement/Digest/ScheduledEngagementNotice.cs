namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled engagement notice is in its life cycle.</summary>
public enum ScheduledEngagementNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One scheduled engagement notice, with what the planners need to act on it.</summary>
public sealed class ScheduledEngagementNotice(string code, string name, int priority, int expiresAt, double hours, double ceiling, string description, int leaveDays, string label, double fte, string reviewer, ScheduledEngagementNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Hours { get; set; } = hours;
    public double Ceiling { get; set; } = ceiling;
    public string Description { get; set; } = description;
    public int LeaveDays { get; set; } = leaveDays;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string Reviewer { get; set; } = reviewer;
    public ScheduledEngagementNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled engagement notice in status draft.</summary>
    public static ScheduledEngagementNotice NewScheduledEngagementNotice(string code, string name)
    {
        return new ScheduledEngagementNotice(
            code,
            name,
            0,
            0,
            0.0,
            0.0,
            "",
            0,
            "",
            0.0,
            "",
            ScheduledEngagementNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEngagementNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEngagementNoticeStatus next)
    {
        switch (Status)
        {
            case ScheduledEngagementNoticeStatus.Draft:
                switch (next)
                {
                    case ScheduledEngagementNoticeStatus.Suspended:
                    case ScheduledEngagementNoticeStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledEngagementNoticeStatus.Pending:
                switch (next)
                {
                    case ScheduledEngagementNoticeStatus.Active:
                        return true;
                }
                return false;
            case ScheduledEngagementNoticeStatus.Active:
                switch (next)
                {
                    case ScheduledEngagementNoticeStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledEngagementNoticeStatus.Suspended:
                switch (next)
                {
                    case ScheduledEngagementNoticeStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledEngagementNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled engagement notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double NetExposure()
    {
        return Math.Round((double)LeaveDays - (double)Priority, 2);
    }

    public ScheduledEngagementNotice Copy()
    {
        return new ScheduledEngagementNotice(
            Code,
            Name,
            Priority,
            ExpiresAt,
            Hours,
            Ceiling,
            Description,
            LeaveDays,
            Label,
            Fte,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
