namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling feedback score is in its life cycle.</summary>
public enum RollingFeedbackScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling feedback score. Its code is unique within a tenant.</summary>
public sealed class RollingFeedbackScore(string code, string name, double budget, string channel, int sequence, int durationMinutes, double rate, int attempts, int graceDays, int batchSize, RollingFeedbackScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Channel { get; set; } = channel;
    public int Sequence { get; set; } = sequence;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public int GraceDays { get; set; } = graceDays;
    public int BatchSize { get; set; } = batchSize;
    public RollingFeedbackScoreStatus Status { get; set; } = status;

    /// <summary>Starts a rolling feedback score in status open.</summary>
    public static RollingFeedbackScore NewRollingFeedbackScore(string code, string name)
    {
        return new RollingFeedbackScore(code, name, 0.0, "", 0, 0, 0.0, 0, 0, 0, RollingFeedbackScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFeedbackScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFeedbackScoreStatus next)
    {
        switch (Status)
        {
            case RollingFeedbackScoreStatus.Open:
                switch (next)
                {
                    case RollingFeedbackScoreStatus.Resolved:
                    case RollingFeedbackScoreStatus.Reopened:
                        return true;
                }
                return false;
            case RollingFeedbackScoreStatus.InReview:
                return next == RollingFeedbackScoreStatus.Reopened;
            case RollingFeedbackScoreStatus.Resolved:
                return next == RollingFeedbackScoreStatus.Reopened || next == RollingFeedbackScoreStatus.Archived;
            case RollingFeedbackScoreStatus.Reopened:
                return next == RollingFeedbackScoreStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingFeedbackScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling feedback score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetExposure()
    {
        return Math.Round(Rate - Budget, 2);
    }

    public RollingFeedbackScore Copy()
    {
        return new RollingFeedbackScore(
            Code,
            Name,
            Budget,
            Channel,
            Sequence,
            DurationMinutes,
            Rate,
            Attempts,
            GraceDays,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
