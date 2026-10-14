namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment batch is in its life cycle.</summary>
public enum AdjustmentBatchStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a adjustment batch and its current status.</summary>
public sealed class AdjustmentBatch(string code, string label, int revision, string ownerId, double discount, string segment, AdjustmentBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string OwnerId { get; set; } = ownerId;
    public double Discount { get; set; } = discount;
    public string Segment { get; set; } = segment;
    public AdjustmentBatchStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment batch in status planned.</summary>
    public static AdjustmentBatch NewAdjustmentBatch(string code, string label)
    {
        return new AdjustmentBatch(code, label, 0, "", 0.0, "", AdjustmentBatchStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentBatchStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentBatchStatus next)
    {
        switch (Status)
        {
            case AdjustmentBatchStatus.Planned:
                switch (next)
                {
                    case AdjustmentBatchStatus.Completed:
                        return true;
                }
                return false;
            case AdjustmentBatchStatus.Scheduled:
                switch (next)
                {
                    case AdjustmentBatchStatus.InProgress:
                        return true;
                }
                return false;
            case AdjustmentBatchStatus.InProgress:
                switch (next)
                {
                    case AdjustmentBatchStatus.Completed:
                    case AdjustmentBatchStatus.Cancelled:
                        return true;
                }
                return false;
            case AdjustmentBatchStatus.Completed:
                switch (next)
                {
                    case AdjustmentBatchStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Revision - Discount, 2);
    }

    public AdjustmentBatch Copy()
    {
        return new AdjustmentBatch(Code, Label, Revision, OwnerId, Discount, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
