namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment threshold is in its life cycle.</summary>
public enum SentimentThresholdStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A sentiment threshold as the sentiment team keeps it.</summary>
public sealed class SentimentThreshold(string code, string title, string label, int createdAt, double ceiling, SentimentThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public double Ceiling { get; set; } = ceiling;
    public SentimentThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment threshold in status requested.</summary>
    public static SentimentThreshold NewSentimentThreshold(string code, string title)
    {
        return new SentimentThreshold(code, title, "", 0, 0.0, SentimentThresholdStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentThresholdStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentThresholdStatus next)
    {
        switch (Status)
        {
            case SentimentThresholdStatus.Requested:
                switch (next)
                {
                    case SentimentThresholdStatus.Revoked:
                    case SentimentThresholdStatus.Expired:
                        return true;
                }
                return false;
            case SentimentThresholdStatus.Granted:
                return next == SentimentThresholdStatus.Revoked;
            case SentimentThresholdStatus.Revoked:
                return next == SentimentThresholdStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SentimentThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Ceiling + (double)CreatedAt, 2);
    }

    public SentimentThreshold Copy()
    {
        return new SentimentThreshold(Code, Title, Label, CreatedAt, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
