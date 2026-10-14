namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal feedback estimate is in its life cycle.</summary>
public enum SeasonalFeedbackEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A seasonal feedback estimate as the feedback team keeps it.</summary>
public sealed class SeasonalFeedbackEstimate(string code, string label, string reviewer, int endsAt, int batchSize, SeasonalFeedbackEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public int EndsAt { get; set; } = endsAt;
    public int BatchSize { get; set; } = batchSize;
    public SeasonalFeedbackEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal feedback estimate in status planned.</summary>
    public static SeasonalFeedbackEstimate NewSeasonalFeedbackEstimate(string code, string label)
    {
        return new SeasonalFeedbackEstimate(code, label, "", 0, 0, SeasonalFeedbackEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFeedbackEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFeedbackEstimateStatus next)
    {
        switch (Status)
        {
            case SeasonalFeedbackEstimateStatus.Planned:
                return next == SeasonalFeedbackEstimateStatus.Completed || next == SeasonalFeedbackEstimateStatus.Cancelled;
            case SeasonalFeedbackEstimateStatus.Scheduled:
                return next == SeasonalFeedbackEstimateStatus.InProgress || next == SeasonalFeedbackEstimateStatus.Cancelled;
            case SeasonalFeedbackEstimateStatus.InProgress:
                switch (next)
                {
                    case SeasonalFeedbackEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case SeasonalFeedbackEstimateStatus.Completed:
                switch (next)
                {
                    case SeasonalFeedbackEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalFeedbackEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal feedback estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round((double)BatchSize + (double)EndsAt, 2);
    }

    public SeasonalFeedbackEstimate Copy()
    {
        return new SeasonalFeedbackEstimate(Code, Label, Reviewer, EndsAt, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
