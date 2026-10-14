namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement notice is in its life cycle.</summary>
public enum EngagementNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One engagement notice, with what the planners need to act on it.</summary>
public sealed class EngagementNotice(string code, string label, string locale, int priority, string reviewer, int revision, EngagementNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int Priority { get; set; } = priority;
    public string Reviewer { get; set; } = reviewer;
    public int Revision { get; set; } = revision;
    public EngagementNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a engagement notice in status created.</summary>
    public static EngagementNotice NewEngagementNotice(string code, string label)
    {
        return new EngagementNotice(code, label, "", 0, "", 0, EngagementNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementNoticeStatus next)
    {
        switch (Status)
        {
            case EngagementNoticeStatus.Created:
                return next == EngagementNoticeStatus.Validated || next == EngagementNoticeStatus.Retired;
            case EngagementNoticeStatus.Validated:
                switch (next)
                {
                    case EngagementNoticeStatus.Published:
                    case EngagementNoticeStatus.Retired:
                        return true;
                }
                return false;
            case EngagementNoticeStatus.Published:
                switch (next)
                {
                    case EngagementNoticeStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Revision - (double)Priority, 2);
    }

    public EngagementNotice Copy()
    {
        return new EngagementNotice(Code, Label, Locale, Priority, Reviewer, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
