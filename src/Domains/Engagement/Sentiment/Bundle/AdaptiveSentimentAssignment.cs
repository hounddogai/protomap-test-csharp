namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive sentiment assignment is in its life cycle.</summary>
public enum AdaptiveSentimentAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive sentiment assignment. Its code is unique within a tenant.</summary>
public sealed class AdaptiveSentimentAssignment(string code, string title, int tenureMonths, string location, double ceiling, double cost, AdaptiveSentimentAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Location { get; set; } = location;
    public double Ceiling { get; set; } = ceiling;
    public double Cost { get; set; } = cost;
    public AdaptiveSentimentAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive sentiment assignment in status open.</summary>
    public static AdaptiveSentimentAssignment NewAdaptiveSentimentAssignment(string code, string title)
    {
        return new AdaptiveSentimentAssignment(code, title, 0, "", 0.0, 0.0, AdaptiveSentimentAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveSentimentAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveSentimentAssignmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveSentimentAssignmentStatus.Open:
                return next == AdaptiveSentimentAssignmentStatus.InReview || next == AdaptiveSentimentAssignmentStatus.Resolved;
            case AdaptiveSentimentAssignmentStatus.InReview:
                return next == AdaptiveSentimentAssignmentStatus.Resolved || next == AdaptiveSentimentAssignmentStatus.Reopened;
            case AdaptiveSentimentAssignmentStatus.Resolved:
                switch (next)
                {
                    case AdaptiveSentimentAssignmentStatus.Reopened:
                    case AdaptiveSentimentAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveSentimentAssignmentStatus.Reopened:
                switch (next)
                {
                    case AdaptiveSentimentAssignmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveSentimentAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive sentiment assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ceiling * (double)TenureMonths, 2);
    }

    public AdaptiveSentimentAssignment Copy()
    {
        return new AdaptiveSentimentAssignment(Code, Title, TenureMonths, Location, Ceiling, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
