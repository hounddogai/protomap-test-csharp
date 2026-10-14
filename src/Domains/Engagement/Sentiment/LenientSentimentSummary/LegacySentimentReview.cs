namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy sentiment review is in its life cycle.</summary>
public enum LegacySentimentReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy sentiment review as the sentiment team keeps it.</summary>
public sealed class LegacySentimentReview(string code, string title, bool pinned, int windowDays, double floorValue, string unit, LegacySentimentReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public int WindowDays { get; set; } = windowDays;
    public double FloorValue { get; set; } = floorValue;
    public string Unit { get; set; } = unit;
    public LegacySentimentReviewStatus Status { get; set; } = status;

    /// <summary>Starts a legacy sentiment review in status planned.</summary>
    public static LegacySentimentReview NewLegacySentimentReview(string code, string title)
    {
        return new LegacySentimentReview(code, title, false, 0, 0.0, "", LegacySentimentReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacySentimentReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacySentimentReviewStatus next)
    {
        switch (Status)
        {
            case LegacySentimentReviewStatus.Planned:
                return next == LegacySentimentReviewStatus.Scheduled || next == LegacySentimentReviewStatus.InProgress;
            case LegacySentimentReviewStatus.Scheduled:
                switch (next)
                {
                    case LegacySentimentReviewStatus.Completed:
                    case LegacySentimentReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacySentimentReviewStatus.InProgress:
                switch (next)
                {
                    case LegacySentimentReviewStatus.Completed:
                        return true;
                }
                return false;
            case LegacySentimentReviewStatus.Completed:
                switch (next)
                {
                    case LegacySentimentReviewStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacySentimentReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy sentiment review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(FloorValue - (double)WindowDays, 2);
    }

    public LegacySentimentReview Copy()
    {
        return new LegacySentimentReview(Code, Title, Pinned, WindowDays, FloorValue, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
