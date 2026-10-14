namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh review is in its life cycle.</summary>
public enum RefreshReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A refresh review. Its code is unique within a tenant.</summary>
public sealed class RefreshReview(string code, string title, string segment, string channel, string costCenter, double score, int startsAt, string ownerId, string summary, string team, RefreshReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public string Channel { get; set; } = channel;
    public string CostCenter { get; set; } = costCenter;
    public double Score { get; set; } = score;
    public int StartsAt { get; set; } = startsAt;
    public string OwnerId { get; set; } = ownerId;
    public string Summary { get; set; } = summary;
    public string Team { get; set; } = team;
    public RefreshReviewStatus Status { get; set; } = status;

    /// <summary>Starts a refresh review in status planned.</summary>
    public static RefreshReview NewRefreshReview(string code, string title)
    {
        return new RefreshReview(code, title, "", "", "", 0.0, 0, "", "", "", RefreshReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshReviewStatus next)
    {
        switch (Status)
        {
            case RefreshReviewStatus.Planned:
                switch (next)
                {
                    case RefreshReviewStatus.Scheduled:
                        return true;
                }
                return false;
            case RefreshReviewStatus.Scheduled:
                switch (next)
                {
                    case RefreshReviewStatus.InProgress:
                        return true;
                }
                return false;
            case RefreshReviewStatus.InProgress:
                switch (next)
                {
                    case RefreshReviewStatus.Completed:
                    case RefreshReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case RefreshReviewStatus.Completed:
                switch (next)
                {
                    case RefreshReviewStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"refresh review"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Score + (double)StartsAt, 2);
    }

    public RefreshReview Copy()
    {
        return new RefreshReview(
            Code,
            Title,
            Segment,
            Channel,
            CostCenter,
            Score,
            StartsAt,
            OwnerId,
            Summary,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
