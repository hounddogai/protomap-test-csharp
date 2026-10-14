namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback checkpoint is in its life cycle.</summary>
public enum FeedbackCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A feedback checkpoint as the feedback team keeps it.</summary>
public sealed class FeedbackCheckpoint(string code, string name, string locale, int sequence, string managerId, string ownerId, int durationMinutes, string currency, string grade, string summary, int version, FeedbackCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public int Sequence { get; set; } = sequence;
    public string ManagerId { get; set; } = managerId;
    public string OwnerId { get; set; } = ownerId;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Currency { get; set; } = currency;
    public string Grade { get; set; } = grade;
    public string Summary { get; set; } = summary;
    public int Version { get; set; } = version;
    public FeedbackCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a feedback checkpoint in status draft.</summary>
    public static FeedbackCheckpoint NewFeedbackCheckpoint(string code, string name)
    {
        return new FeedbackCheckpoint(code, name, "", 0, "", "", 0, "", "", "", 0, FeedbackCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackCheckpointStatus next)
    {
        switch (Status)
        {
            case FeedbackCheckpointStatus.Draft:
                return next == FeedbackCheckpointStatus.Suspended;
            case FeedbackCheckpointStatus.Pending:
                return next == FeedbackCheckpointStatus.Closed;
            case FeedbackCheckpointStatus.Active:
                switch (next)
                {
                    case FeedbackCheckpointStatus.Suspended:
                        return true;
                }
                return false;
            case FeedbackCheckpointStatus.Suspended:
                return next == FeedbackCheckpointStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FeedbackCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Version - (double)Sequence, 2);
    }

    public FeedbackCheckpoint Copy()
    {
        return new FeedbackCheckpoint(
            Code,
            Name,
            Locale,
            Sequence,
            ManagerId,
            OwnerId,
            DurationMinutes,
            Currency,
            Grade,
            Summary,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
