namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback access badge estimate is in its life cycle.</summary>
public enum FallbackAccessBadgeEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A fallback access badge estimate. Its code is unique within a tenant.</summary>
public sealed class FallbackAccessBadgeEstimate(string code, string label, double maximum, double score, List<string> tags, string country, string team, FallbackAccessBadgeEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public double Score { get; set; } = score;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public string Team { get; set; } = team;
    public FallbackAccessBadgeEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a fallback access badge estimate in status idle.</summary>
    public static FallbackAccessBadgeEstimate NewFallbackAccessBadgeEstimate(string code, string label)
    {
        return new FallbackAccessBadgeEstimate(
            code,
            label,
            0.0,
            0.0,
            new List<string>(),
            "",
            "",
            FallbackAccessBadgeEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAccessBadgeEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAccessBadgeEstimateStatus next)
    {
        switch (Status)
        {
            case FallbackAccessBadgeEstimateStatus.Idle:
                switch (next)
                {
                    case FallbackAccessBadgeEstimateStatus.Draining:
                    case FallbackAccessBadgeEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackAccessBadgeEstimateStatus.Warming:
                return next == FallbackAccessBadgeEstimateStatus.Draining;
            case FallbackAccessBadgeEstimateStatus.Ready:
                switch (next)
                {
                    case FallbackAccessBadgeEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackAccessBadgeEstimateStatus.Draining:
                switch (next)
                {
                    case FallbackAccessBadgeEstimateStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackAccessBadgeEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback access badge estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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
        return Math.Round(Maximum + Score, 2);
    }

    public FallbackAccessBadgeEstimate Copy()
    {
        return new FallbackAccessBadgeEstimate(Code, Label, Maximum, Score, Tags, Country, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
