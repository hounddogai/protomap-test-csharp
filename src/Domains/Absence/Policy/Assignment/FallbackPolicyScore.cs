namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback policy score is in its life cycle.</summary>
public enum FallbackPolicyScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a fallback policy score and its current status.</summary>
public sealed class FallbackPolicyScore(string code, string label, double hours, double cost, string category, double budget, string segment, string reviewer, int durationMinutes, List<string> tags, string name, FallbackPolicyScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public double Cost { get; set; } = cost;
    public string Category { get; set; } = category;
    public double Budget { get; set; } = budget;
    public string Segment { get; set; } = segment;
    public string Reviewer { get; set; } = reviewer;
    public int DurationMinutes { get; set; } = durationMinutes;
    public List<string> Tags { get; set; } = tags;
    public string Name { get; set; } = name;
    public FallbackPolicyScoreStatus Status { get; set; } = status;

    /// <summary>Starts a fallback policy score in status open.</summary>
    public static FallbackPolicyScore NewFallbackPolicyScore(string code, string label)
    {
        return new FallbackPolicyScore(
            code,
            label,
            0.0,
            0.0,
            "",
            0.0,
            "",
            "",
            0,
            new List<string>(),
            "",
            FallbackPolicyScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPolicyScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPolicyScoreStatus next)
    {
        switch (Status)
        {
            case FallbackPolicyScoreStatus.Open:
                switch (next)
                {
                    case FallbackPolicyScoreStatus.InReview:
                    case FallbackPolicyScoreStatus.Resolved:
                        return true;
                }
                return false;
            case FallbackPolicyScoreStatus.InReview:
                switch (next)
                {
                    case FallbackPolicyScoreStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackPolicyScoreStatus.Resolved:
                return next == FallbackPolicyScoreStatus.Reopened || next == FallbackPolicyScoreStatus.Archived;
            case FallbackPolicyScoreStatus.Reopened:
                return next == FallbackPolicyScoreStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackPolicyScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback policy score {Code} cannot move from {Status} to {next}");
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

    public double ProjectedLoad()
    {
        return Math.Round(Budget - Cost, 2);
    }

    public FallbackPolicyScore Copy()
    {
        return new FallbackPolicyScore(
            Code,
            Label,
            Hours,
            Cost,
            Category,
            Budget,
            Segment,
            Reviewer,
            DurationMinutes,
            Tags,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
