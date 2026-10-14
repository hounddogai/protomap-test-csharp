namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification review is in its life cycle.</summary>
public enum CertificationReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a certification review and its current status.</summary>
public sealed class CertificationReview(string code, string title, string ownerId, bool active, int revision, double budget, string reviewer, CertificationReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public bool Active { get; set; } = active;
    public int Revision { get; set; } = revision;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public CertificationReviewStatus Status { get; set; } = status;

    /// <summary>Starts a certification review in status open.</summary>
    public static CertificationReview NewCertificationReview(string code, string title)
    {
        return new CertificationReview(code, title, "", false, 0, 0.0, "", CertificationReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationReviewStatus next)
    {
        switch (Status)
        {
            case CertificationReviewStatus.Open:
                return next == CertificationReviewStatus.Resolved;
            case CertificationReviewStatus.InReview:
                switch (next)
                {
                    case CertificationReviewStatus.Resolved:
                    case CertificationReviewStatus.Reopened:
                        return true;
                }
                return false;
            case CertificationReviewStatus.Resolved:
                return next == CertificationReviewStatus.Reopened || next == CertificationReviewStatus.Archived;
            case CertificationReviewStatus.Reopened:
                switch (next)
                {
                    case CertificationReviewStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Revision + Budget, 2);
    }

    public CertificationReview Copy()
    {
        return new CertificationReview(Code, Title, OwnerId, Active, Revision, Budget, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
