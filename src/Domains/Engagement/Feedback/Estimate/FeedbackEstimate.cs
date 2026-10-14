namespace Acme.Workforce.Engagement.Feedback.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback estimate is in its life cycle.</summary>
public enum FeedbackEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A feedback estimate as the feedback team keeps it.</summary>
public sealed class FeedbackEstimate(string code, string title, string skill, string currency, int expiresAt, string description, bool pinned, FeedbackEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public string Currency { get; set; } = currency;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Description { get; set; } = description;
    public bool Pinned { get; set; } = pinned;
    public FeedbackEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a feedback estimate in status proposed.</summary>
    public static FeedbackEstimate NewFeedbackEstimate(string code, string title)
    {
        return new FeedbackEstimate(code, title, "", "", 0, "", false, FeedbackEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackEstimateStatus next)
    {
        switch (Status)
        {
            case FeedbackEstimateStatus.Proposed:
                switch (next)
                {
                    case FeedbackEstimateStatus.Approved:
                    case FeedbackEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case FeedbackEstimateStatus.Approved:
                return next == FeedbackEstimateStatus.Rejected;
            case FeedbackEstimateStatus.Rejected:
                switch (next)
                {
                    case FeedbackEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public FeedbackEstimate Copy()
    {
        return new FeedbackEstimate(Code, Title, Skill, Currency, ExpiresAt, Description, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
