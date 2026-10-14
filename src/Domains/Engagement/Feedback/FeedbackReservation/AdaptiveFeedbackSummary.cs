namespace Acme.Workforce.Engagement.Feedback.FeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive feedback summary is in its life cycle.</summary>
public enum AdaptiveFeedbackSummaryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adaptive feedback summary as the feedback team keeps it.</summary>
public sealed class AdaptiveFeedbackSummary(string code, string title, string notes, int windowDays, double hours, AdaptiveFeedbackSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public int WindowDays { get; set; } = windowDays;
    public double Hours { get; set; } = hours;
    public AdaptiveFeedbackSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive feedback summary in status planned.</summary>
    public static AdaptiveFeedbackSummary NewAdaptiveFeedbackSummary(string code, string title)
    {
        return new AdaptiveFeedbackSummary(code, title, "", 0, 0.0, AdaptiveFeedbackSummaryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveFeedbackSummaryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveFeedbackSummaryStatus next)
    {
        switch (Status)
        {
            case AdaptiveFeedbackSummaryStatus.Planned:
                switch (next)
                {
                    case AdaptiveFeedbackSummaryStatus.InProgress:
                        return true;
                }
                return false;
            case AdaptiveFeedbackSummaryStatus.Scheduled:
                switch (next)
                {
                    case AdaptiveFeedbackSummaryStatus.InProgress:
                    case AdaptiveFeedbackSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveFeedbackSummaryStatus.InProgress:
                switch (next)
                {
                    case AdaptiveFeedbackSummaryStatus.Completed:
                    case AdaptiveFeedbackSummaryStatus.Cancelled:
                        return true;
                }
                return false;
            case AdaptiveFeedbackSummaryStatus.Completed:
                switch (next)
                {
                    case AdaptiveFeedbackSummaryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveFeedbackSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive feedback summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)WindowDays - Hours, 2);
    }

    public AdaptiveFeedbackSummary Copy()
    {
        return new AdaptiveFeedbackSummary(Code, Title, Notes, WindowDays, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
