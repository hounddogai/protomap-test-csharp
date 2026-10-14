namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback tier is in its life cycle.</summary>
public enum FeedbackTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One feedback tier, with what the planners need to act on it.</summary>
public sealed class FeedbackTier(string code, string title, double amount, string summary, string employeeId, FeedbackTierStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Summary { get; set; } = summary;
    public string EmployeeId { get; set; } = employeeId;
    public FeedbackTierStatus Status { get; set; } = status;

    /// <summary>Starts a feedback tier in status new.</summary>
    public static FeedbackTier NewFeedbackTier(string code, string title)
    {
        return new FeedbackTier(code, title, 0.0, "", "", FeedbackTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackTierStatus next)
    {
        switch (Status)
        {
            case FeedbackTierStatus.New:
                return next == FeedbackTierStatus.Done || next == FeedbackTierStatus.Failed;
            case FeedbackTierStatus.Queued:
                return next == FeedbackTierStatus.Done;
            case FeedbackTierStatus.Running:
                return next == FeedbackTierStatus.Done || next == FeedbackTierStatus.Failed;
            case FeedbackTierStatus.Done:
                return next == FeedbackTierStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FeedbackTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public FeedbackTier Copy()
    {
        return new FeedbackTier(Code, Title, Amount, Summary, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
