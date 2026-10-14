namespace Acme.Workforce.Engagement.Feedback.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback milestone is in its life cycle.</summary>
public enum FeedbackMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a feedback milestone and its current status.</summary>
public sealed class FeedbackMilestone(string code, string title, int windowDays, int priority, string salaryBand, int batchSize, string location, int limit, FeedbackMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public int Priority { get; set; } = priority;
    public string SalaryBand { get; set; } = salaryBand;
    public int BatchSize { get; set; } = batchSize;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public FeedbackMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a feedback milestone in status idle.</summary>
    public static FeedbackMilestone NewFeedbackMilestone(string code, string title)
    {
        return new FeedbackMilestone(code, title, 0, 0, "", 0, "", 0, FeedbackMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackMilestoneStatus next)
    {
        switch (Status)
        {
            case FeedbackMilestoneStatus.Idle:
                switch (next)
                {
                    case FeedbackMilestoneStatus.Warming:
                        return true;
                }
                return false;
            case FeedbackMilestoneStatus.Warming:
                switch (next)
                {
                    case FeedbackMilestoneStatus.Draining:
                        return true;
                }
                return false;
            case FeedbackMilestoneStatus.Ready:
                switch (next)
                {
                    case FeedbackMilestoneStatus.Draining:
                    case FeedbackMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case FeedbackMilestoneStatus.Draining:
                switch (next)
                {
                    case FeedbackMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback milestone"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)WindowDays * (double)Priority, 2);
    }

    public FeedbackMilestone Copy()
    {
        return new FeedbackMilestone(
            Code,
            Title,
            WindowDays,
            Priority,
            SalaryBand,
            BatchSize,
            Location,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
