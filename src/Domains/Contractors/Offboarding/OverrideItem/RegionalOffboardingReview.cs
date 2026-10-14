namespace Acme.Workforce.Contractors.Offboarding.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional offboarding review is in its life cycle.</summary>
public enum RegionalOffboardingReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional offboarding review as the offboarding team keeps it.</summary>
public sealed class RegionalOffboardingReview(string code, string title, double margin, int leaveDays, double discount, string segment, string employeeId, List<string> tags, RegionalOffboardingReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public double Discount { get; set; } = discount;
    public string Segment { get; set; } = segment;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public RegionalOffboardingReviewStatus Status { get; set; } = status;

    /// <summary>Starts a regional offboarding review in status proposed.</summary>
    public static RegionalOffboardingReview NewRegionalOffboardingReview(string code, string title)
    {
        return new RegionalOffboardingReview(
            code,
            title,
            0.0,
            0,
            0.0,
            "",
            "",
            new List<string>(),
            RegionalOffboardingReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalOffboardingReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalOffboardingReviewStatus next)
    {
        switch (Status)
        {
            case RegionalOffboardingReviewStatus.Proposed:
                return next == RegionalOffboardingReviewStatus.Approved || next == RegionalOffboardingReviewStatus.Withdrawn;
            case RegionalOffboardingReviewStatus.Approved:
                return next == RegionalOffboardingReviewStatus.Rejected;
            case RegionalOffboardingReviewStatus.Rejected:
                switch (next)
                {
                    case RegionalOffboardingReviewStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalOffboardingReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional offboarding review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetValue()
    {
        return Math.Round((double)LeaveDays + Discount, 2);
    }

    public RegionalOffboardingReview Copy()
    {
        return new RegionalOffboardingReview(
            Code,
            Title,
            Margin,
            LeaveDays,
            Discount,
            Segment,
            EmployeeId,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
