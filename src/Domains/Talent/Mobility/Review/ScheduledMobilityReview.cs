namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled mobility review is in its life cycle.</summary>
public enum ScheduledMobilityReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One scheduled mobility review, with what the planners need to act on it.</summary>
public sealed class ScheduledMobilityReview(string code, string title, double maximum, string channel, string location, string externalRef, int durationMinutes, ScheduledMobilityReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Channel { get; set; } = channel;
    public string Location { get; set; } = location;
    public string ExternalRef { get; set; } = externalRef;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ScheduledMobilityReviewStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled mobility review in status new.</summary>
    public static ScheduledMobilityReview NewScheduledMobilityReview(string code, string title)
    {
        return new ScheduledMobilityReview(code, title, 0.0, "", "", "", 0, ScheduledMobilityReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledMobilityReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledMobilityReviewStatus next)
    {
        switch (Status)
        {
            case ScheduledMobilityReviewStatus.New:
                return next == ScheduledMobilityReviewStatus.Running || next == ScheduledMobilityReviewStatus.Done;
            case ScheduledMobilityReviewStatus.Queued:
                return next == ScheduledMobilityReviewStatus.Done || next == ScheduledMobilityReviewStatus.Failed;
            case ScheduledMobilityReviewStatus.Running:
                switch (next)
                {
                    case ScheduledMobilityReviewStatus.Done:
                    case ScheduledMobilityReviewStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledMobilityReviewStatus.Done:
                switch (next)
                {
                    case ScheduledMobilityReviewStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledMobilityReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled mobility review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Maximum + (double)DurationMinutes, 2);
    }

    public ScheduledMobilityReview Copy()
    {
        return new ScheduledMobilityReview(
            Code,
            Title,
            Maximum,
            Channel,
            Location,
            ExternalRef,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
