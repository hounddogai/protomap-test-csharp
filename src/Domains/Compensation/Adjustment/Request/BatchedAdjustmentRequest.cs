namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched adjustment request is in its life cycle.</summary>
public enum BatchedAdjustmentRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A batched adjustment request as the adjustment team keeps it.</summary>
public sealed class BatchedAdjustmentRequest(string code, string title, string badge, string skill, string label, int quantity, string reviewer, bool pinned, BatchedAdjustmentRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Skill { get; set; } = skill;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public string Reviewer { get; set; } = reviewer;
    public bool Pinned { get; set; } = pinned;
    public BatchedAdjustmentRequestStatus Status { get; set; } = status;

    /// <summary>Starts a batched adjustment request in status planned.</summary>
    public static BatchedAdjustmentRequest NewBatchedAdjustmentRequest(string code, string title)
    {
        return new BatchedAdjustmentRequest(
            code,
            title,
            "",
            "",
            "",
            0,
            "",
            false,
            BatchedAdjustmentRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAdjustmentRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAdjustmentRequestStatus next)
    {
        switch (Status)
        {
            case BatchedAdjustmentRequestStatus.Planned:
                return next == BatchedAdjustmentRequestStatus.Completed;
            case BatchedAdjustmentRequestStatus.Scheduled:
                switch (next)
                {
                    case BatchedAdjustmentRequestStatus.Completed:
                    case BatchedAdjustmentRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedAdjustmentRequestStatus.InProgress:
                switch (next)
                {
                    case BatchedAdjustmentRequestStatus.Completed:
                    case BatchedAdjustmentRequestStatus.Cancelled:
                        return true;
                }
                return false;
            case BatchedAdjustmentRequestStatus.Completed:
                return next == BatchedAdjustmentRequestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedAdjustmentRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched adjustment request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public BatchedAdjustmentRequest Copy()
    {
        return new BatchedAdjustmentRequest(Code, Title, Badge, Skill, Label, Quantity, Reviewer, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
