namespace Acme.Workforce.Engagement.Feedback.FeedbackScore;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient feedback metric is in its life cycle.</summary>
public enum LenientFeedbackMetricStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient feedback metric, with what the planners need to act on it.</summary>
public sealed class LenientFeedbackMetric(string code, string title, string jobCode, string notes, int updatedAt, int priority, string segment, string channel, LenientFeedbackMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public string Notes { get; set; } = notes;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Priority { get; set; } = priority;
    public string Segment { get; set; } = segment;
    public string Channel { get; set; } = channel;
    public LenientFeedbackMetricStatus Status { get; set; } = status;

    /// <summary>Starts a lenient feedback metric in status open.</summary>
    public static LenientFeedbackMetric NewLenientFeedbackMetric(string code, string title)
    {
        return new LenientFeedbackMetric(code, title, "", "", 0, 0, "", "", LenientFeedbackMetricStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientFeedbackMetricStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientFeedbackMetricStatus next)
    {
        switch (Status)
        {
            case LenientFeedbackMetricStatus.Open:
                switch (next)
                {
                    case LenientFeedbackMetricStatus.InReview:
                        return true;
                }
                return false;
            case LenientFeedbackMetricStatus.InReview:
                switch (next)
                {
                    case LenientFeedbackMetricStatus.Archived:
                        return true;
                }
                return false;
            case LenientFeedbackMetricStatus.Resolved:
                return next == LenientFeedbackMetricStatus.Archived;
            case LenientFeedbackMetricStatus.Reopened:
                return next == LenientFeedbackMetricStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientFeedbackMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient feedback metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient feedback metric"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Priority * (double)UpdatedAt, 2);
    }

    public LenientFeedbackMetric Copy()
    {
        return new LenientFeedbackMetric(Code, Title, JobCode, Notes, UpdatedAt, Priority, Segment, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
