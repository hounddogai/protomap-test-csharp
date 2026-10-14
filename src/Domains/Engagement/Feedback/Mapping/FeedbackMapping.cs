namespace Acme.Workforce.Engagement.Feedback.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback mapping is in its life cycle.</summary>
public enum FeedbackMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A feedback mapping as the feedback team keeps it.</summary>
public sealed class FeedbackMapping(string code, string title, bool approved, string reviewer, int leaveDays, double margin, double discount, double rating, string timezone, FeedbackMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public string Reviewer { get; set; } = reviewer;
    public int LeaveDays { get; set; } = leaveDays;
    public double Margin { get; set; } = margin;
    public double Discount { get; set; } = discount;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public FeedbackMappingStatus Status { get; set; } = status;

    /// <summary>Starts a feedback mapping in status new.</summary>
    public static FeedbackMapping NewFeedbackMapping(string code, string title)
    {
        return new FeedbackMapping(code, title, false, "", 0, 0.0, 0.0, 0.0, "", FeedbackMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackMappingStatus next)
    {
        switch (Status)
        {
            case FeedbackMappingStatus.New:
                return next == FeedbackMappingStatus.Done || next == FeedbackMappingStatus.Failed;
            case FeedbackMappingStatus.Queued:
                return next == FeedbackMappingStatus.Done || next == FeedbackMappingStatus.Failed;
            case FeedbackMappingStatus.Running:
                return next == FeedbackMappingStatus.Done;
            case FeedbackMappingStatus.Done:
                return next == FeedbackMappingStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FeedbackMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"feedback mapping"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rating - Margin, 2);
    }

    public FeedbackMapping Copy()
    {
        return new FeedbackMapping(
            Code,
            Title,
            Approved,
            Reviewer,
            LeaveDays,
            Margin,
            Discount,
            Rating,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
