namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched bonus pool threshold is in its life cycle.</summary>
public enum BatchedBonusPoolThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a batched bonus pool threshold and its current status.</summary>
public sealed class BatchedBonusPoolThreshold(string code, string title, string externalRef, double fte, bool pinned, string unit, int attempts, int quantity, double ceiling, string notes, string region, BatchedBonusPoolThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public double Fte { get; set; } = fte;
    public bool Pinned { get; set; } = pinned;
    public string Unit { get; set; } = unit;
    public int Attempts { get; set; } = attempts;
    public int Quantity { get; set; } = quantity;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public string Region { get; set; } = region;
    public BatchedBonusPoolThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a batched bonus pool threshold in status open.</summary>
    public static BatchedBonusPoolThreshold NewBatchedBonusPoolThreshold(string code, string title)
    {
        return new BatchedBonusPoolThreshold(
            code,
            title,
            "",
            0.0,
            false,
            "",
            0,
            0,
            0.0,
            "",
            "",
            BatchedBonusPoolThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBonusPoolThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBonusPoolThresholdStatus next)
    {
        switch (Status)
        {
            case BatchedBonusPoolThresholdStatus.Open:
                switch (next)
                {
                    case BatchedBonusPoolThresholdStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedBonusPoolThresholdStatus.InReview:
                return next == BatchedBonusPoolThresholdStatus.Resolved || next == BatchedBonusPoolThresholdStatus.Reopened;
            case BatchedBonusPoolThresholdStatus.Resolved:
                return next == BatchedBonusPoolThresholdStatus.Reopened || next == BatchedBonusPoolThresholdStatus.Archived;
            case BatchedBonusPoolThresholdStatus.Reopened:
                switch (next)
                {
                    case BatchedBonusPoolThresholdStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedBonusPoolThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched bonus pool threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched bonus pool threshold"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Attempts + Fte, 2);
    }

    public BatchedBonusPoolThreshold Copy()
    {
        return new BatchedBonusPoolThreshold(
            Code,
            Title,
            ExternalRef,
            Fte,
            Pinned,
            Unit,
            Attempts,
            Quantity,
            Ceiling,
            Notes,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
