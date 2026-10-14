namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched adjustment schedule is in its life cycle.</summary>
public enum BatchedAdjustmentScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One batched adjustment schedule, with what the planners need to act on it.</summary>
public sealed class BatchedAdjustmentSchedule(string code, string label, int expiresAt, double minimum, int capacity, string description, BatchedAdjustmentScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Minimum { get; set; } = minimum;
    public int Capacity { get; set; } = capacity;
    public string Description { get; set; } = description;
    public BatchedAdjustmentScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a batched adjustment schedule in status new.</summary>
    public static BatchedAdjustmentSchedule NewBatchedAdjustmentSchedule(string code, string label)
    {
        return new BatchedAdjustmentSchedule(code, label, 0, 0.0, 0, "", BatchedAdjustmentScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAdjustmentScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAdjustmentScheduleStatus next)
    {
        switch (Status)
        {
            case BatchedAdjustmentScheduleStatus.New:
                return next == BatchedAdjustmentScheduleStatus.Done || next == BatchedAdjustmentScheduleStatus.Failed;
            case BatchedAdjustmentScheduleStatus.Queued:
                return next == BatchedAdjustmentScheduleStatus.Done;
            case BatchedAdjustmentScheduleStatus.Running:
                return next == BatchedAdjustmentScheduleStatus.Failed;
            case BatchedAdjustmentScheduleStatus.Done:
                return next == BatchedAdjustmentScheduleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedAdjustmentScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched adjustment schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetValue()
    {
        return Math.Round((double)Capacity * (double)ExpiresAt, 2);
    }

    public BatchedAdjustmentSchedule Copy()
    {
        return new BatchedAdjustmentSchedule(Code, Label, ExpiresAt, Minimum, Capacity, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
