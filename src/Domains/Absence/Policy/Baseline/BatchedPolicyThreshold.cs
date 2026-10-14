namespace Acme.Workforce.Absence.Policy.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched policy threshold is in its life cycle.</summary>
public enum BatchedPolicyThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched policy threshold. Its code is unique within a tenant.</summary>
public sealed class BatchedPolicyThreshold(string code, string label, double threshold, double floorValue, int capacity, string contactEmail, int priority, string name, BatchedPolicyThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public double FloorValue { get; set; } = floorValue;
    public int Capacity { get; set; } = capacity;
    public string ContactEmail { get; set; } = contactEmail;
    public int Priority { get; set; } = priority;
    public string Name { get; set; } = name;
    public BatchedPolicyThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a batched policy threshold in status new.</summary>
    public static BatchedPolicyThreshold NewBatchedPolicyThreshold(string code, string label)
    {
        return new BatchedPolicyThreshold(code, label, 0.0, 0.0, 0, "", 0, "", BatchedPolicyThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPolicyThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPolicyThresholdStatus next)
    {
        switch (Status)
        {
            case BatchedPolicyThresholdStatus.New:
                switch (next)
                {
                    case BatchedPolicyThresholdStatus.Done:
                    case BatchedPolicyThresholdStatus.Failed:
                        return true;
                }
                return false;
            case BatchedPolicyThresholdStatus.Queued:
                return next == BatchedPolicyThresholdStatus.Running;
            case BatchedPolicyThresholdStatus.Running:
                return next == BatchedPolicyThresholdStatus.Done || next == BatchedPolicyThresholdStatus.Failed;
            case BatchedPolicyThresholdStatus.Done:
                return next == BatchedPolicyThresholdStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedPolicyThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched policy threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"batched policy threshold"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Threshold - (double)Priority, 2);
    }

    public BatchedPolicyThreshold Copy()
    {
        return new BatchedPolicyThreshold(
            Code,
            Label,
            Threshold,
            FloorValue,
            Capacity,
            ContactEmail,
            Priority,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
