namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment record is in its life cycle.</summary>
public enum SentimentRecordStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A sentiment record. Its code is unique within a tenant.</summary>
public sealed class SentimentRecord(string code, string title, double threshold, int quantity, int updatedAt, string timezone, int priority, string locale, string source, double amount, SentimentRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public int Quantity { get; set; } = quantity;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Timezone { get; set; } = timezone;
    public int Priority { get; set; } = priority;
    public string Locale { get; set; } = locale;
    public string Source { get; set; } = source;
    public double Amount { get; set; } = amount;
    public SentimentRecordStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment record in status requested.</summary>
    public static SentimentRecord NewSentimentRecord(string code, string title)
    {
        return new SentimentRecord(code, title, 0.0, 0, 0, "", 0, "", "", 0.0, SentimentRecordStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentRecordStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentRecordStatus next)
    {
        switch (Status)
        {
            case SentimentRecordStatus.Requested:
                switch (next)
                {
                    case SentimentRecordStatus.Granted:
                    case SentimentRecordStatus.Revoked:
                        return true;
                }
                return false;
            case SentimentRecordStatus.Granted:
                return next == SentimentRecordStatus.Revoked || next == SentimentRecordStatus.Expired;
            case SentimentRecordStatus.Revoked:
                return next == SentimentRecordStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SentimentRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"sentiment record"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt - (double)Priority, 2);
    }

    public SentimentRecord Copy()
    {
        return new SentimentRecord(
            Code,
            Title,
            Threshold,
            Quantity,
            UpdatedAt,
            Timezone,
            Priority,
            Locale,
            Source,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
