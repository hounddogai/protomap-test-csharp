namespace Acme.Workforce.Engagement.Feedback.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched feedback score is in its life cycle.</summary>
public enum BatchedFeedbackScoreStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a batched feedback score and its current status.</summary>
public sealed class BatchedFeedbackScore(string code, string title, string reviewer, double cost, string currency, double rate, double ceiling, int version, string externalRef, int windowDays, double hours, BatchedFeedbackScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Cost { get; set; } = cost;
    public string Currency { get; set; } = currency;
    public double Rate { get; set; } = rate;
    public double Ceiling { get; set; } = ceiling;
    public int Version { get; set; } = version;
    public string ExternalRef { get; set; } = externalRef;
    public int WindowDays { get; set; } = windowDays;
    public double Hours { get; set; } = hours;
    public BatchedFeedbackScoreStatus Status { get; set; } = status;

    /// <summary>Starts a batched feedback score in status open.</summary>
    public static BatchedFeedbackScore NewBatchedFeedbackScore(string code, string title)
    {
        return new BatchedFeedbackScore(
            code,
            title,
            "",
            0.0,
            "",
            0.0,
            0.0,
            0,
            "",
            0,
            0.0,
            BatchedFeedbackScoreStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedFeedbackScoreStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedFeedbackScoreStatus next)
    {
        switch (Status)
        {
            case BatchedFeedbackScoreStatus.Open:
                switch (next)
                {
                    case BatchedFeedbackScoreStatus.InReview:
                    case BatchedFeedbackScoreStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedFeedbackScoreStatus.InReview:
                return next == BatchedFeedbackScoreStatus.Archived;
            case BatchedFeedbackScoreStatus.Resolved:
                switch (next)
                {
                    case BatchedFeedbackScoreStatus.Reopened:
                    case BatchedFeedbackScoreStatus.Archived:
                        return true;
                }
                return false;
            case BatchedFeedbackScoreStatus.Reopened:
                switch (next)
                {
                    case BatchedFeedbackScoreStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedFeedbackScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched feedback score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"batched feedback score"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Ceiling - Hours, 2);
    }

    public BatchedFeedbackScore Copy()
    {
        return new BatchedFeedbackScore(
            Code,
            Title,
            Reviewer,
            Cost,
            Currency,
            Rate,
            Ceiling,
            Version,
            ExternalRef,
            WindowDays,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
