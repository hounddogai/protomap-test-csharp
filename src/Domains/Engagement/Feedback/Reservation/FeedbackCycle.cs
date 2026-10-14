namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback cycle is in its life cycle.</summary>
public enum FeedbackCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A feedback cycle as the feedback team keeps it.</summary>
public sealed class FeedbackCycle(string code, string title, int leaveDays, string source, bool archived, int retries, bool visible, Dictionary<string, string> attributes, int durationMinutes, FeedbackCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Source { get; set; } = source;
    public bool Archived { get; set; } = archived;
    public int Retries { get; set; } = retries;
    public bool Visible { get; set; } = visible;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public FeedbackCycleStatus Status { get; set; } = status;

    /// <summary>Starts a feedback cycle in status idle.</summary>
    public static FeedbackCycle NewFeedbackCycle(string code, string title)
    {
        return new FeedbackCycle(
            code,
            title,
            0,
            "",
            false,
            0,
            false,
            new Dictionary<string, string>(),
            0,
            FeedbackCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackCycleStatus next)
    {
        switch (Status)
        {
            case FeedbackCycleStatus.Idle:
                return next == FeedbackCycleStatus.Warming || next == FeedbackCycleStatus.Draining;
            case FeedbackCycleStatus.Warming:
                switch (next)
                {
                    case FeedbackCycleStatus.Ready:
                    case FeedbackCycleStatus.Stopped:
                        return true;
                }
                return false;
            case FeedbackCycleStatus.Ready:
                switch (next)
                {
                    case FeedbackCycleStatus.Draining:
                    case FeedbackCycleStatus.Stopped:
                        return true;
                }
                return false;
            case FeedbackCycleStatus.Draining:
                return next == FeedbackCycleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FeedbackCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedValue()
    {
        return Math.Round((double)DurationMinutes + (double)LeaveDays, 2);
    }

    public FeedbackCycle Copy()
    {
        return new FeedbackCycle(
            Code,
            Title,
            LeaveDays,
            Source,
            Archived,
            Retries,
            Visible,
            Attributes,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
