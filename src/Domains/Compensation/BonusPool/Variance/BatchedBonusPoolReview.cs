namespace Acme.Workforce.Compensation.BonusPool.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched bonus pool review is in its life cycle.</summary>
public enum BatchedBonusPoolReviewStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One batched bonus pool review, with what the planners need to act on it.</summary>
public sealed class BatchedBonusPoolReview(string code, string title, int startsAt, double hours, int revision, string managerId, string externalRef, string checksum, double ceiling, int updatedAt, BatchedBonusPoolReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public double Hours { get; set; } = hours;
    public int Revision { get; set; } = revision;
    public string ManagerId { get; set; } = managerId;
    public string ExternalRef { get; set; } = externalRef;
    public string Checksum { get; set; } = checksum;
    public double Ceiling { get; set; } = ceiling;
    public int UpdatedAt { get; set; } = updatedAt;
    public BatchedBonusPoolReviewStatus Status { get; set; } = status;

    /// <summary>Starts a batched bonus pool review in status planned.</summary>
    public static BatchedBonusPoolReview NewBatchedBonusPoolReview(string code, string title)
    {
        return new BatchedBonusPoolReview(
            code,
            title,
            0,
            0.0,
            0,
            "",
            "",
            "",
            0.0,
            0,
            BatchedBonusPoolReviewStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBonusPoolReviewStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBonusPoolReviewStatus next)
    {
        switch (Status)
        {
            case BatchedBonusPoolReviewStatus.Planned:
                switch (next)
                {
                    case BatchedBonusPoolReviewStatus.Scheduled:
                    case BatchedBonusPoolReviewStatus.Completed:
                        return true;
                }
                return false;
            case BatchedBonusPoolReviewStatus.Scheduled:
                return next == BatchedBonusPoolReviewStatus.Cancelled;
            case BatchedBonusPoolReviewStatus.InProgress:
                return next == BatchedBonusPoolReviewStatus.Completed;
            case BatchedBonusPoolReviewStatus.Completed:
                return next == BatchedBonusPoolReviewStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedBonusPoolReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched bonus pool review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"batched bonus pool review"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Ceiling * (double)StartsAt, 2);
    }

    public BatchedBonusPoolReview Copy()
    {
        return new BatchedBonusPoolReview(
            Code,
            Title,
            StartsAt,
            Hours,
            Revision,
            ManagerId,
            ExternalRef,
            Checksum,
            Ceiling,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
