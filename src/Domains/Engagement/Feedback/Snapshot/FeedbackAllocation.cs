namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback allocation is in its life cycle.</summary>
public enum FeedbackAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A feedback allocation as the feedback team keeps it.</summary>
public sealed class FeedbackAllocation(string code, string title, bool pinned, double maximum, string reviewer, FeedbackAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public double Maximum { get; set; } = maximum;
    public string Reviewer { get; set; } = reviewer;
    public FeedbackAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a feedback allocation in status planned.</summary>
    public static FeedbackAllocation NewFeedbackAllocation(string code, string title)
    {
        return new FeedbackAllocation(code, title, false, 0.0, "", FeedbackAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackAllocationStatus next)
    {
        switch (Status)
        {
            case FeedbackAllocationStatus.Planned:
                switch (next)
                {
                    case FeedbackAllocationStatus.Scheduled:
                    case FeedbackAllocationStatus.InProgress:
                        return true;
                }
                return false;
            case FeedbackAllocationStatus.Scheduled:
                switch (next)
                {
                    case FeedbackAllocationStatus.Cancelled:
                        return true;
                }
                return false;
            case FeedbackAllocationStatus.InProgress:
                return next == FeedbackAllocationStatus.Completed || next == FeedbackAllocationStatus.Cancelled;
            case FeedbackAllocationStatus.Completed:
                return next == FeedbackAllocationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FeedbackAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback allocation"} {Code}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public FeedbackAllocation Copy()
    {
        return new FeedbackAllocation(Code, Title, Pinned, Maximum, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
