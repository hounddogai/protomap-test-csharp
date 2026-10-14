namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception review is in its life cycle.</summary>
public enum ExceptionReviewStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a exception review and its current status.</summary>
public sealed class ExceptionReview(string code, string name, double discount, double score, string locale, string title, int startsAt, int createdAt, double ratio, string category, ExceptionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public double Score { get; set; } = score;
    public string Locale { get; set; } = locale;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public string Category { get; set; } = category;
    public ExceptionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a exception review in status requested.</summary>
    public static ExceptionReview NewExceptionReview(string code, string name)
    {
        return new ExceptionReview(code, name, 0.0, 0.0, "", "", 0, 0, 0.0, "", ExceptionReviewStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionReviewStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionReviewStatus next)
    {
        switch (Status)
        {
            case ExceptionReviewStatus.Requested:
                switch (next)
                {
                    case ExceptionReviewStatus.Granted:
                    case ExceptionReviewStatus.Expired:
                        return true;
                }
                return false;
            case ExceptionReviewStatus.Granted:
                switch (next)
                {
                    case ExceptionReviewStatus.Expired:
                        return true;
                }
                return false;
            case ExceptionReviewStatus.Revoked:
                switch (next)
                {
                    case ExceptionReviewStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ExceptionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ratio * (double)StartsAt, 2);
    }

    public ExceptionReview Copy()
    {
        return new ExceptionReview(
            Code,
            Name,
            Discount,
            Score,
            Locale,
            Title,
            StartsAt,
            CreatedAt,
            Ratio,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
