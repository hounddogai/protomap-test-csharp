namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement review is in its life cycle.</summary>
public enum EngagementReviewStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A engagement review. Its code is unique within a tenant.</summary>
public sealed class EngagementReview(string code, string name, double minimum, string title, int version, EngagementReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public EngagementReviewStatus Status { get; set; } = status;

    /// <summary>Starts a engagement review in status proposed.</summary>
    public static EngagementReview NewEngagementReview(string code, string name)
    {
        return new EngagementReview(code, name, 0.0, "", 0, EngagementReviewStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementReviewStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementReviewStatus next)
    {
        switch (Status)
        {
            case EngagementReviewStatus.Proposed:
                return next == EngagementReviewStatus.Approved || next == EngagementReviewStatus.Rejected;
            case EngagementReviewStatus.Approved:
                switch (next)
                {
                    case EngagementReviewStatus.Rejected:
                    case EngagementReviewStatus.Withdrawn:
                        return true;
                }
                return false;
            case EngagementReviewStatus.Rejected:
                switch (next)
                {
                    case EngagementReviewStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Version + Minimum, 2);
    }

    public EngagementReview Copy()
    {
        return new EngagementReview(Code, Name, Minimum, Title, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
