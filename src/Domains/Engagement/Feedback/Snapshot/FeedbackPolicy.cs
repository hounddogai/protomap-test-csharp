namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback policy is in its life cycle.</summary>
public enum FeedbackPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One feedback policy, with what the planners need to act on it.</summary>
public sealed class FeedbackPolicy(string code, string title, double ratio, string locale, double budget, double margin, FeedbackPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Locale { get; set; } = locale;
    public double Budget { get; set; } = budget;
    public double Margin { get; set; } = margin;
    public FeedbackPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a feedback policy in status planned.</summary>
    public static FeedbackPolicy NewFeedbackPolicy(string code, string title)
    {
        return new FeedbackPolicy(code, title, 0.0, "", 0.0, 0.0, FeedbackPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackPolicyStatus next)
    {
        switch (Status)
        {
            case FeedbackPolicyStatus.Planned:
                switch (next)
                {
                    case FeedbackPolicyStatus.InProgress:
                        return true;
                }
                return false;
            case FeedbackPolicyStatus.Scheduled:
                return next == FeedbackPolicyStatus.InProgress || next == FeedbackPolicyStatus.Completed;
            case FeedbackPolicyStatus.InProgress:
                switch (next)
                {
                    case FeedbackPolicyStatus.Completed:
                        return true;
                }
                return false;
            case FeedbackPolicyStatus.Completed:
                return next == FeedbackPolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FeedbackPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback policy"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Margin + Ratio, 2);
    }

    public FeedbackPolicy Copy()
    {
        return new FeedbackPolicy(Code, Title, Ratio, Locale, Budget, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
