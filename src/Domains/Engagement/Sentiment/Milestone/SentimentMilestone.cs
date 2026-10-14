namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment milestone is in its life cycle.</summary>
public enum SentimentMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A sentiment milestone. Its code is unique within a tenant.</summary>
public sealed class SentimentMilestone(string code, string label, string channel, string notes, string grade, bool pinned, int updatedAt, SentimentMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Notes { get; set; } = notes;
    public string Grade { get; set; } = grade;
    public bool Pinned { get; set; } = pinned;
    public int UpdatedAt { get; set; } = updatedAt;
    public SentimentMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment milestone in status requested.</summary>
    public static SentimentMilestone NewSentimentMilestone(string code, string label)
    {
        return new SentimentMilestone(code, label, "", "", "", false, 0, SentimentMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentMilestoneStatus next)
    {
        switch (Status)
        {
            case SentimentMilestoneStatus.Requested:
                return next == SentimentMilestoneStatus.Revoked;
            case SentimentMilestoneStatus.Granted:
                return next == SentimentMilestoneStatus.Expired;
            case SentimentMilestoneStatus.Revoked:
                return next == SentimentMilestoneStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SentimentMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"sentiment milestone"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public SentimentMilestone Copy()
    {
        return new SentimentMilestone(Code, Label, Channel, Notes, Grade, Pinned, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
