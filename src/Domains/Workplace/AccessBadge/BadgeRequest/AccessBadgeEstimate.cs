namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge estimate is in its life cycle.</summary>
public enum AccessBadgeEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One access badge estimate, with what the planners need to act on it.</summary>
public sealed class AccessBadgeEstimate(string code, string title, double floorValue, double margin, double ceiling, double cost, AccessBadgeEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Margin { get; set; } = margin;
    public double Ceiling { get; set; } = ceiling;
    public double Cost { get; set; } = cost;
    public AccessBadgeEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a access badge estimate in status open.</summary>
    public static AccessBadgeEstimate NewAccessBadgeEstimate(string code, string title)
    {
        return new AccessBadgeEstimate(code, title, 0.0, 0.0, 0.0, 0.0, AccessBadgeEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeEstimateStatus next)
    {
        switch (Status)
        {
            case AccessBadgeEstimateStatus.Open:
                return next == AccessBadgeEstimateStatus.Resolved;
            case AccessBadgeEstimateStatus.InReview:
                switch (next)
                {
                    case AccessBadgeEstimateStatus.Reopened:
                    case AccessBadgeEstimateStatus.Archived:
                        return true;
                }
                return false;
            case AccessBadgeEstimateStatus.Resolved:
                switch (next)
                {
                    case AccessBadgeEstimateStatus.Reopened:
                    case AccessBadgeEstimateStatus.Archived:
                        return true;
                }
                return false;
            case AccessBadgeEstimateStatus.Reopened:
                return next == AccessBadgeEstimateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AccessBadgeEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round(FloorValue - Cost, 2);
    }

    public AccessBadgeEstimate Copy()
    {
        return new AccessBadgeEstimate(Code, Title, FloorValue, Margin, Ceiling, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
