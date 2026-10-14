namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band review is in its life cycle.</summary>
public enum PayBandReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a pay band review and its current status.</summary>
public sealed class PayBandReview(string code, string name, int createdAt, string reviewer, int retries, int expiresAt, PayBandReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public string Reviewer { get; set; } = reviewer;
    public int Retries { get; set; } = retries;
    public int ExpiresAt { get; set; } = expiresAt;
    public PayBandReviewStatus Status { get; set; } = status;

    /// <summary>Starts a pay band review in status open.</summary>
    public static PayBandReview NewPayBandReview(string code, string name)
    {
        return new PayBandReview(code, name, 0, "", 0, 0, PayBandReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandReviewStatus next)
    {
        switch (Status)
        {
            case PayBandReviewStatus.Open:
                switch (next)
                {
                    case PayBandReviewStatus.Reopened:
                        return true;
                }
                return false;
            case PayBandReviewStatus.InReview:
                switch (next)
                {
                    case PayBandReviewStatus.Resolved:
                    case PayBandReviewStatus.Archived:
                        return true;
                }
                return false;
            case PayBandReviewStatus.Resolved:
                return next == PayBandReviewStatus.Archived;
            case PayBandReviewStatus.Reopened:
                switch (next)
                {
                    case PayBandReviewStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pay band review"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Retries + (double)ExpiresAt, 2);
    }

    public PayBandReview Copy()
    {
        return new PayBandReview(Code, Name, CreatedAt, Reviewer, Retries, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
