namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment schedule is in its life cycle.</summary>
public enum SentimentScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A sentiment schedule as the sentiment team keeps it.</summary>
public sealed class SentimentSchedule(string code, string name, double floorValue, string externalRef, int attempts, string description, string notes, double threshold, string ownerId, SentimentScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string ExternalRef { get; set; } = externalRef;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public string Notes { get; set; } = notes;
    public double Threshold { get; set; } = threshold;
    public string OwnerId { get; set; } = ownerId;
    public SentimentScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment schedule in status open.</summary>
    public static SentimentSchedule NewSentimentSchedule(string code, string name)
    {
        return new SentimentSchedule(code, name, 0.0, "", 0, "", "", 0.0, "", SentimentScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentScheduleStatus next)
    {
        switch (Status)
        {
            case SentimentScheduleStatus.Open:
                switch (next)
                {
                    case SentimentScheduleStatus.Archived:
                        return true;
                }
                return false;
            case SentimentScheduleStatus.InReview:
                switch (next)
                {
                    case SentimentScheduleStatus.Resolved:
                    case SentimentScheduleStatus.Reopened:
                        return true;
                }
                return false;
            case SentimentScheduleStatus.Resolved:
                return next == SentimentScheduleStatus.Reopened || next == SentimentScheduleStatus.Archived;
            case SentimentScheduleStatus.Reopened:
                switch (next)
                {
                    case SentimentScheduleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(FloorValue - Threshold, 2);
    }

    public SentimentSchedule Copy()
    {
        return new SentimentSchedule(
            Code,
            Name,
            FloorValue,
            ExternalRef,
            Attempts,
            Description,
            Notes,
            Threshold,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
