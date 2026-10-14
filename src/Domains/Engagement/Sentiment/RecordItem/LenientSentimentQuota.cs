namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient sentiment quota is in its life cycle.</summary>
public enum LenientSentimentQuotaStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a lenient sentiment quota and its current status.</summary>
public sealed class LenientSentimentQuota(string code, string title, string badge, int endsAt, int expiresAt, string notes, double maximum, LenientSentimentQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Notes { get; set; } = notes;
    public double Maximum { get; set; } = maximum;
    public LenientSentimentQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a lenient sentiment quota in status new.</summary>
    public static LenientSentimentQuota NewLenientSentimentQuota(string code, string title)
    {
        return new LenientSentimentQuota(code, title, "", 0, 0, "", 0.0, LenientSentimentQuotaStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSentimentQuotaStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSentimentQuotaStatus next)
    {
        switch (Status)
        {
            case LenientSentimentQuotaStatus.New:
                return next == LenientSentimentQuotaStatus.Running || next == LenientSentimentQuotaStatus.Done;
            case LenientSentimentQuotaStatus.Queued:
                return next == LenientSentimentQuotaStatus.Done || next == LenientSentimentQuotaStatus.Failed;
            case LenientSentimentQuotaStatus.Running:
                return next == LenientSentimentQuotaStatus.Done;
            case LenientSentimentQuotaStatus.Done:
                return next == LenientSentimentQuotaStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LenientSentimentQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient sentiment quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Maximum + (double)ExpiresAt, 2);
    }

    public LenientSentimentQuota Copy()
    {
        return new LenientSentimentQuota(Code, Title, Badge, EndsAt, ExpiresAt, Notes, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
