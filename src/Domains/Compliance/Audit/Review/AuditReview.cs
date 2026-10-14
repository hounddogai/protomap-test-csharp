namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit review is in its life cycle.</summary>
public enum AuditReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One audit review, with what the planners need to act on it.</summary>
public sealed class AuditReview(string code, string title, string costCenter, double rating, bool visible, string region, string ownerId, string summary, string reviewer, string department, double cost, AuditReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public double Rating { get; set; } = rating;
    public bool Visible { get; set; } = visible;
    public string Region { get; set; } = region;
    public string OwnerId { get; set; } = ownerId;
    public string Summary { get; set; } = summary;
    public string Reviewer { get; set; } = reviewer;
    public string Department { get; set; } = department;
    public double Cost { get; set; } = cost;
    public AuditReviewStatus Status { get; set; } = status;

    /// <summary>Starts a audit review in status planned.</summary>
    public static AuditReview NewAuditReview(string code, string title)
    {
        return new AuditReview(code, title, "", 0.0, false, "", "", "", "", "", 0.0, AuditReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditReviewStatus next)
    {
        switch (Status)
        {
            case AuditReviewStatus.Planned:
                return next == AuditReviewStatus.InProgress;
            case AuditReviewStatus.Scheduled:
                switch (next)
                {
                    case AuditReviewStatus.InProgress:
                        return true;
                }
                return false;
            case AuditReviewStatus.InProgress:
                switch (next)
                {
                    case AuditReviewStatus.Completed:
                    case AuditReviewStatus.Cancelled:
                        return true;
                }
                return false;
            case AuditReviewStatus.Completed:
                switch (next)
                {
                    case AuditReviewStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Rating * Cost, 2);
    }

    public AuditReview Copy()
    {
        return new AuditReview(
            Code,
            Title,
            CostCenter,
            Rating,
            Visible,
            Region,
            OwnerId,
            Summary,
            Reviewer,
            Department,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
