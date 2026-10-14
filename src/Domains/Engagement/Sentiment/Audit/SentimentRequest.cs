namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment request is in its life cycle.</summary>
public enum SentimentRequestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A sentiment request as the sentiment team keeps it.</summary>
public sealed class SentimentRequest(string code, string title, string currency, string source, string ownerId, int updatedAt, double score, double budget, int durationMinutes, int createdAt, string timezone, SentimentRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Score { get; set; } = score;
    public double Budget { get; set; } = budget;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int CreatedAt { get; set; } = createdAt;
    public string Timezone { get; set; } = timezone;
    public SentimentRequestStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment request in status open.</summary>
    public static SentimentRequest NewSentimentRequest(string code, string title)
    {
        return new SentimentRequest(code, title, "", "", "", 0, 0.0, 0.0, 0, 0, "", SentimentRequestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentRequestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentRequestStatus next)
    {
        switch (Status)
        {
            case SentimentRequestStatus.Open:
                return next == SentimentRequestStatus.Reopened;
            case SentimentRequestStatus.InReview:
                return next == SentimentRequestStatus.Resolved || next == SentimentRequestStatus.Reopened;
            case SentimentRequestStatus.Resolved:
                return next == SentimentRequestStatus.Archived;
            case SentimentRequestStatus.Reopened:
                return next == SentimentRequestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SentimentRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)CreatedAt + Budget, 2);
    }

    public SentimentRequest Copy()
    {
        return new SentimentRequest(
            Code,
            Title,
            Currency,
            Source,
            OwnerId,
            UpdatedAt,
            Score,
            Budget,
            DurationMinutes,
            CreatedAt,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
