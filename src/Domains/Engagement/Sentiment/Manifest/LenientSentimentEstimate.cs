namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient sentiment estimate is in its life cycle.</summary>
public enum LenientSentimentEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient sentiment estimate. Its code is unique within a tenant.</summary>
public sealed class LenientSentimentEstimate(string code, string title, int createdAt, double floorValue, int expiresAt, LenientSentimentEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public double FloorValue { get; set; } = floorValue;
    public int ExpiresAt { get; set; } = expiresAt;
    public LenientSentimentEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a lenient sentiment estimate in status draft.</summary>
    public static LenientSentimentEstimate NewLenientSentimentEstimate(string code, string title)
    {
        return new LenientSentimentEstimate(code, title, 0, 0.0, 0, LenientSentimentEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSentimentEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSentimentEstimateStatus next)
    {
        switch (Status)
        {
            case LenientSentimentEstimateStatus.Draft:
                return next == LenientSentimentEstimateStatus.Closed;
            case LenientSentimentEstimateStatus.Pending:
                return next == LenientSentimentEstimateStatus.Suspended || next == LenientSentimentEstimateStatus.Closed;
            case LenientSentimentEstimateStatus.Active:
                return next == LenientSentimentEstimateStatus.Suspended || next == LenientSentimentEstimateStatus.Closed;
            case LenientSentimentEstimateStatus.Suspended:
                switch (next)
                {
                    case LenientSentimentEstimateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientSentimentEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient sentiment estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient sentiment estimate"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)ExpiresAt - (double)CreatedAt, 2);
    }

    public LenientSentimentEstimate Copy()
    {
        return new LenientSentimentEstimate(Code, Title, CreatedAt, FloorValue, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
