namespace Acme.Workforce.Compensation.BonusPool.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool review is in its life cycle.</summary>
public enum BonusPoolReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One bonus pool review, with what the planners need to act on it.</summary>
public sealed class BonusPoolReview(string code, string title, double minimum, double score, int createdAt, double hours, string label, BonusPoolReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public double Score { get; set; } = score;
    public int CreatedAt { get; set; } = createdAt;
    public double Hours { get; set; } = hours;
    public string Label { get; set; } = label;
    public BonusPoolReviewStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool review in status idle.</summary>
    public static BonusPoolReview NewBonusPoolReview(string code, string title)
    {
        return new BonusPoolReview(code, title, 0.0, 0.0, 0, 0.0, "", BonusPoolReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolReviewStatus next)
    {
        switch (Status)
        {
            case BonusPoolReviewStatus.Idle:
                return next == BonusPoolReviewStatus.Ready;
            case BonusPoolReviewStatus.Warming:
                switch (next)
                {
                    case BonusPoolReviewStatus.Ready:
                    case BonusPoolReviewStatus.Stopped:
                        return true;
                }
                return false;
            case BonusPoolReviewStatus.Ready:
                return next == BonusPoolReviewStatus.Stopped;
            case BonusPoolReviewStatus.Draining:
                return next == BonusPoolReviewStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BonusPoolReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Minimum * Score, 2);
    }

    public BonusPoolReview Copy()
    {
        return new BonusPoolReview(Code, Title, Minimum, Score, CreatedAt, Hours, Label, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
