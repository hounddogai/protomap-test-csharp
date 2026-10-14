namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled engagement review is in its life cycle.</summary>
public enum ScheduledEngagementReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One scheduled engagement review, with what the planners need to act on it.</summary>
public sealed class ScheduledEngagementReview(string code, string label, bool active, string channel, string source, string location, string costCenter, string contactEmail, int expiresAt, ScheduledEngagementReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string Channel { get; set; } = channel;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public string ContactEmail { get; set; } = contactEmail;
    public int ExpiresAt { get; set; } = expiresAt;
    public ScheduledEngagementReviewStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled engagement review in status requested.</summary>
    public static ScheduledEngagementReview NewScheduledEngagementReview(string code, string label)
    {
        return new ScheduledEngagementReview(
            code,
            label,
            false,
            "",
            "",
            "",
            "",
            "",
            0,
            ScheduledEngagementReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEngagementReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEngagementReviewStatus next)
    {
        switch (Status)
        {
            case ScheduledEngagementReviewStatus.Requested:
                return next == ScheduledEngagementReviewStatus.Granted;
            case ScheduledEngagementReviewStatus.Granted:
                switch (next)
                {
                    case ScheduledEngagementReviewStatus.Expired:
                        return true;
                }
                return false;
            case ScheduledEngagementReviewStatus.Revoked:
                return next == ScheduledEngagementReviewStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ScheduledEngagementReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled engagement review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"scheduled engagement review"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public ScheduledEngagementReview Copy()
    {
        return new ScheduledEngagementReview(
            Code,
            Label,
            Active,
            Channel,
            Source,
            Location,
            CostCenter,
            ContactEmail,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
