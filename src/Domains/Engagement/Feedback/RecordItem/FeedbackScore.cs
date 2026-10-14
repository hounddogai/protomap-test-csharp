namespace Acme.Workforce.Engagement.Feedback.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback score is in its life cycle.</summary>
public enum FeedbackScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A feedback score. Its code is unique within a tenant.</summary>
public sealed class FeedbackScore(string code, string name, string source, int expiresAt, string summary, int quantity, double budget, FeedbackScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Summary { get; set; } = summary;
    public int Quantity { get; set; } = quantity;
    public double Budget { get; set; } = budget;
    public FeedbackScoreStatus Status { get; set; } = status;

    /// <summary>Starts a feedback score in status draft.</summary>
    public static FeedbackScore NewFeedbackScore(string code, string name)
    {
        return new FeedbackScore(code, name, "", 0, "", 0, 0.0, FeedbackScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackScoreStatus next)
    {
        switch (Status)
        {
            case FeedbackScoreStatus.Draft:
                switch (next)
                {
                    case FeedbackScoreStatus.Active:
                        return true;
                }
                return false;
            case FeedbackScoreStatus.Pending:
                switch (next)
                {
                    case FeedbackScoreStatus.Suspended:
                        return true;
                }
                return false;
            case FeedbackScoreStatus.Active:
                switch (next)
                {
                    case FeedbackScoreStatus.Suspended:
                        return true;
                }
                return false;
            case FeedbackScoreStatus.Suspended:
                return next == FeedbackScoreStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FeedbackScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Quantity + (double)ExpiresAt, 2);
    }

    public FeedbackScore Copy()
    {
        return new FeedbackScore(Code, Name, Source, ExpiresAt, Summary, Quantity, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
