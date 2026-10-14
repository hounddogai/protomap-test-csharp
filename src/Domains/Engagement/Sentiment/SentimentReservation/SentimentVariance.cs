namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment variance is in its life cycle.</summary>
public enum SentimentVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A sentiment variance. Its code is unique within a tenant.</summary>
public sealed class SentimentVariance(string code, string title, double score, double threshold, string summary, SentimentVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public double Threshold { get; set; } = threshold;
    public string Summary { get; set; } = summary;
    public SentimentVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment variance in status planned.</summary>
    public static SentimentVariance NewSentimentVariance(string code, string title)
    {
        return new SentimentVariance(code, title, 0.0, 0.0, "", SentimentVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentVarianceStatus next)
    {
        switch (Status)
        {
            case SentimentVarianceStatus.Planned:
                return next == SentimentVarianceStatus.Scheduled || next == SentimentVarianceStatus.Completed;
            case SentimentVarianceStatus.Scheduled:
                switch (next)
                {
                    case SentimentVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case SentimentVarianceStatus.InProgress:
                switch (next)
                {
                    case SentimentVarianceStatus.Completed:
                    case SentimentVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case SentimentVarianceStatus.Completed:
                return next == SentimentVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SentimentVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment variance"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Threshold - Score, 2);
    }

    public SentimentVariance Copy()
    {
        return new SentimentVariance(Code, Title, Score, Threshold, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
