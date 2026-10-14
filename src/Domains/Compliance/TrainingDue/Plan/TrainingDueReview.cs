namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a training due review is in its life cycle.</summary>
public enum TrainingDueReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a training due review and its current status.</summary>
public sealed class TrainingDueReview(string code, string label, int limit, double budget, int leaveDays, bool active, string unit, string title, double score, int expiresAt, string channel, TrainingDueReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public double Budget { get; set; } = budget;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Active { get; set; } = active;
    public string Unit { get; set; } = unit;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public TrainingDueReviewStatus Status { get; set; } = status;

    /// <summary>Starts a training due review in status new.</summary>
    public static TrainingDueReview NewTrainingDueReview(string code, string label)
    {
        return new TrainingDueReview(code, label, 0, 0.0, 0, false, "", "", 0.0, 0, "", TrainingDueReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TrainingDueReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TrainingDueReviewStatus next)
    {
        switch (Status)
        {
            case TrainingDueReviewStatus.New:
                return next == TrainingDueReviewStatus.Queued || next == TrainingDueReviewStatus.Running;
            case TrainingDueReviewStatus.Queued:
                return next == TrainingDueReviewStatus.Running || next == TrainingDueReviewStatus.Failed;
            case TrainingDueReviewStatus.Running:
                return next == TrainingDueReviewStatus.Done || next == TrainingDueReviewStatus.Failed;
            case TrainingDueReviewStatus.Done:
                switch (next)
                {
                    case TrainingDueReviewStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TrainingDueReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"training due review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)LeaveDays * Score, 2);
    }

    public TrainingDueReview Copy()
    {
        return new TrainingDueReview(
            Code,
            Label,
            Limit,
            Budget,
            LeaveDays,
            Active,
            Unit,
            Title,
            Score,
            ExpiresAt,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
