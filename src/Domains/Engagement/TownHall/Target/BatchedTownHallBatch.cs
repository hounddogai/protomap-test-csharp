namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched town hall batch is in its life cycle.</summary>
public enum BatchedTownHallBatchStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a batched town hall batch and its current status.</summary>
public sealed class BatchedTownHallBatch(string code, string label, string phone, double weight, string badge, BatchedTownHallBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public double Weight { get; set; } = weight;
    public string Badge { get; set; } = badge;
    public BatchedTownHallBatchStatus Status { get; set; } = status;

    /// <summary>Starts a batched town hall batch in status idle.</summary>
    public static BatchedTownHallBatch NewBatchedTownHallBatch(string code, string label)
    {
        return new BatchedTownHallBatch(code, label, "", 0.0, "", BatchedTownHallBatchStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTownHallBatchStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTownHallBatchStatus next)
    {
        switch (Status)
        {
            case BatchedTownHallBatchStatus.Idle:
                switch (next)
                {
                    case BatchedTownHallBatchStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTownHallBatchStatus.Warming:
                switch (next)
                {
                    case BatchedTownHallBatchStatus.Ready:
                    case BatchedTownHallBatchStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTownHallBatchStatus.Ready:
                switch (next)
                {
                    case BatchedTownHallBatchStatus.Draining:
                    case BatchedTownHallBatchStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTownHallBatchStatus.Draining:
                return next == BatchedTownHallBatchStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedTownHallBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched town hall batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"batched town hall batch"} {Code}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public BatchedTownHallBatch Copy()
    {
        return new BatchedTownHallBatch(Code, Label, Phone, Weight, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
