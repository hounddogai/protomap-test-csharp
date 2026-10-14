namespace Acme.Workforce.Contractors.Engagement.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement segment is in its life cycle.</summary>
public enum EngagementSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One engagement segment, with what the planners need to act on it.</summary>
public sealed class EngagementSegment(string code, string title, string reviewer, string summary, string channel, double amount, EngagementSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public string Summary { get; set; } = summary;
    public string Channel { get; set; } = channel;
    public double Amount { get; set; } = amount;
    public EngagementSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a engagement segment in status requested.</summary>
    public static EngagementSegment NewEngagementSegment(string code, string title)
    {
        return new EngagementSegment(code, title, "", "", "", 0.0, EngagementSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementSegmentStatus next)
    {
        switch (Status)
        {
            case EngagementSegmentStatus.Requested:
                return next == EngagementSegmentStatus.Expired;
            case EngagementSegmentStatus.Granted:
                return next == EngagementSegmentStatus.Revoked || next == EngagementSegmentStatus.Expired;
            case EngagementSegmentStatus.Revoked:
                switch (next)
                {
                    case EngagementSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public EngagementSegment Copy()
    {
        return new EngagementSegment(Code, Title, Reviewer, Summary, Channel, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
