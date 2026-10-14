namespace Acme.Workforce.Engagement.Recognition.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition milestone is in its life cycle.</summary>
public enum RecognitionMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One recognition milestone, with what the planners need to act on it.</summary>
public sealed class RecognitionMilestone(string code, string title, string region, string locale, int createdAt, int limit, double margin, double score, int graceDays, RecognitionMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Locale { get; set; } = locale;
    public int CreatedAt { get; set; } = createdAt;
    public int Limit { get; set; } = limit;
    public double Margin { get; set; } = margin;
    public double Score { get; set; } = score;
    public int GraceDays { get; set; } = graceDays;
    public RecognitionMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a recognition milestone in status idle.</summary>
    public static RecognitionMilestone NewRecognitionMilestone(string code, string title)
    {
        return new RecognitionMilestone(code, title, "", "", 0, 0, 0.0, 0.0, 0, RecognitionMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionMilestoneStatus next)
    {
        switch (Status)
        {
            case RecognitionMilestoneStatus.Idle:
                switch (next)
                {
                    case RecognitionMilestoneStatus.Warming:
                    case RecognitionMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case RecognitionMilestoneStatus.Warming:
                return next == RecognitionMilestoneStatus.Ready;
            case RecognitionMilestoneStatus.Ready:
                return next == RecognitionMilestoneStatus.Draining || next == RecognitionMilestoneStatus.Stopped;
            case RecognitionMilestoneStatus.Draining:
                switch (next)
                {
                    case RecognitionMilestoneStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)GraceDays + (double)CreatedAt, 2);
    }

    public RecognitionMilestone Copy()
    {
        return new RecognitionMilestone(
            Code,
            Title,
            Region,
            Locale,
            CreatedAt,
            Limit,
            Margin,
            Score,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
