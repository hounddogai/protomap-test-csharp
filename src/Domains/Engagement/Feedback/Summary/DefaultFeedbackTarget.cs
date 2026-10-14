namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default feedback target is in its life cycle.</summary>
public enum DefaultFeedbackTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a default feedback target and its current status.</summary>
public sealed class DefaultFeedbackTarget(string code, string title, string externalRef, double margin, double maximum, DefaultFeedbackTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public double Margin { get; set; } = margin;
    public double Maximum { get; set; } = maximum;
    public DefaultFeedbackTargetStatus Status { get; set; } = status;

    /// <summary>Starts a default feedback target in status planned.</summary>
    public static DefaultFeedbackTarget NewDefaultFeedbackTarget(string code, string title)
    {
        return new DefaultFeedbackTarget(code, title, "", 0.0, 0.0, DefaultFeedbackTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultFeedbackTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultFeedbackTargetStatus next)
    {
        switch (Status)
        {
            case DefaultFeedbackTargetStatus.Planned:
                return next == DefaultFeedbackTargetStatus.InProgress || next == DefaultFeedbackTargetStatus.Completed;
            case DefaultFeedbackTargetStatus.Scheduled:
                switch (next)
                {
                    case DefaultFeedbackTargetStatus.Completed:
                        return true;
                }
                return false;
            case DefaultFeedbackTargetStatus.InProgress:
                return next == DefaultFeedbackTargetStatus.Completed || next == DefaultFeedbackTargetStatus.Cancelled;
            case DefaultFeedbackTargetStatus.Completed:
                switch (next)
                {
                    case DefaultFeedbackTargetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultFeedbackTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default feedback target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Maximum * Margin, 2);
    }

    public DefaultFeedbackTarget Copy()
    {
        return new DefaultFeedbackTarget(Code, Title, ExternalRef, Margin, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
