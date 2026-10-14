namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge review is in its life cycle.</summary>
public enum AccessBadgeReviewStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A access badge review. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeReview(string code, string label, string grade, int attempts, int limit, string skill, AccessBadgeReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public int Attempts { get; set; } = attempts;
    public int Limit { get; set; } = limit;
    public string Skill { get; set; } = skill;
    public AccessBadgeReviewStatus Status { get; set; } = status;

    /// <summary>Starts a access badge review in status open.</summary>
    public static AccessBadgeReview NewAccessBadgeReview(string code, string label)
    {
        return new AccessBadgeReview(code, label, "", 0, 0, "", AccessBadgeReviewStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeReviewStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeReviewStatus next)
    {
        switch (Status)
        {
            case AccessBadgeReviewStatus.Open:
                switch (next)
                {
                    case AccessBadgeReviewStatus.Archived:
                        return true;
                }
                return false;
            case AccessBadgeReviewStatus.InReview:
                switch (next)
                {
                    case AccessBadgeReviewStatus.Resolved:
                        return true;
                }
                return false;
            case AccessBadgeReviewStatus.Resolved:
                switch (next)
                {
                    case AccessBadgeReviewStatus.Reopened:
                    case AccessBadgeReviewStatus.Archived:
                        return true;
                }
                return false;
            case AccessBadgeReviewStatus.Reopened:
                switch (next)
                {
                    case AccessBadgeReviewStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Limit * (double)Attempts, 2);
    }

    public AccessBadgeReview Copy()
    {
        return new AccessBadgeReview(Code, Label, Grade, Attempts, Limit, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
