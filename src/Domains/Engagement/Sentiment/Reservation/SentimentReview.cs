namespace Acme.Workforce.Engagement.Sentiment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment review is in its life cycle.</summary>
public enum SentimentReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a sentiment review and its current status.</summary>
public sealed class SentimentReview(string code, string name, int limit, string currency, string locale, SentimentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string Currency { get; set; } = currency;
    public string Locale { get; set; } = locale;
    public SentimentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment review in status idle.</summary>
    public static SentimentReview NewSentimentReview(string code, string name)
    {
        return new SentimentReview(code, name, 0, "", "", SentimentReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentReviewStatus next)
    {
        switch (Status)
        {
            case SentimentReviewStatus.Idle:
                switch (next)
                {
                    case SentimentReviewStatus.Stopped:
                        return true;
                }
                return false;
            case SentimentReviewStatus.Warming:
                return next == SentimentReviewStatus.Draining;
            case SentimentReviewStatus.Ready:
                return next == SentimentReviewStatus.Stopped;
            case SentimentReviewStatus.Draining:
                return next == SentimentReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SentimentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public SentimentReview Copy()
    {
        return new SentimentReview(Code, Name, Limit, Currency, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
