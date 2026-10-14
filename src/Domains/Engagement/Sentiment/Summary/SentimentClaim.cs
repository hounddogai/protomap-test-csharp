namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment claim is in its life cycle.</summary>
public enum SentimentClaimStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a sentiment claim and its current status.</summary>
public sealed class SentimentClaim(string code, string title, string label, bool active, string category, string segment, double score, SentimentClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string Category { get; set; } = category;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public SentimentClaimStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment claim in status proposed.</summary>
    public static SentimentClaim NewSentimentClaim(string code, string title)
    {
        return new SentimentClaim(code, title, "", false, "", "", 0.0, SentimentClaimStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentClaimStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentClaimStatus next)
    {
        switch (Status)
        {
            case SentimentClaimStatus.Proposed:
                switch (next)
                {
                    case SentimentClaimStatus.Approved:
                    case SentimentClaimStatus.Withdrawn:
                        return true;
                }
                return false;
            case SentimentClaimStatus.Approved:
                return next == SentimentClaimStatus.Rejected;
            case SentimentClaimStatus.Rejected:
                return next == SentimentClaimStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SentimentClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"sentiment claim"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public SentimentClaim Copy()
    {
        return new SentimentClaim(Code, Title, Label, Active, Category, Segment, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
