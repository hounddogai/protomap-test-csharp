namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback rule is in its life cycle.</summary>
public enum FeedbackRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One feedback rule, with what the planners need to act on it.</summary>
public sealed class FeedbackRule(string code, string name, bool pinned, int leaveDays, string ownerId, string notes, FeedbackRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public string OwnerId { get; set; } = ownerId;
    public string Notes { get; set; } = notes;
    public FeedbackRuleStatus Status { get; set; } = status;

    /// <summary>Starts a feedback rule in status new.</summary>
    public static FeedbackRule NewFeedbackRule(string code, string name)
    {
        return new FeedbackRule(code, name, false, 0, "", "", FeedbackRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackRuleStatus next)
    {
        switch (Status)
        {
            case FeedbackRuleStatus.New:
                switch (next)
                {
                    case FeedbackRuleStatus.Running:
                    case FeedbackRuleStatus.Done:
                        return true;
                }
                return false;
            case FeedbackRuleStatus.Queued:
                return next == FeedbackRuleStatus.Done || next == FeedbackRuleStatus.Failed;
            case FeedbackRuleStatus.Running:
                return next == FeedbackRuleStatus.Done;
            case FeedbackRuleStatus.Done:
                return next == FeedbackRuleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FeedbackRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public FeedbackRule Copy()
    {
        return new FeedbackRule(Code, Name, Pinned, LeaveDays, OwnerId, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
