namespace Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched offboarding allocation is in its life cycle.</summary>
public enum BatchedOffboardingAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a batched offboarding allocation and its current status.</summary>
public sealed class BatchedOffboardingAllocation(string code, string label, double hours, int batchSize, bool approved, double margin, string timezone, List<string> tags, BatchedOffboardingAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int BatchSize { get; set; } = batchSize;
    public bool Approved { get; set; } = approved;
    public double Margin { get; set; } = margin;
    public string Timezone { get; set; } = timezone;
    public List<string> Tags { get; set; } = tags;
    public BatchedOffboardingAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a batched offboarding allocation in status planned.</summary>
    public static BatchedOffboardingAllocation NewBatchedOffboardingAllocation(string code, string label)
    {
        return new BatchedOffboardingAllocation(
            code,
            label,
            0.0,
            0,
            false,
            0.0,
            "",
            new List<string>(),
            BatchedOffboardingAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedOffboardingAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedOffboardingAllocationStatus next)
    {
        switch (Status)
        {
            case BatchedOffboardingAllocationStatus.Planned:
                return next == BatchedOffboardingAllocationStatus.Cancelled;
            case BatchedOffboardingAllocationStatus.Scheduled:
                return next == BatchedOffboardingAllocationStatus.InProgress || next == BatchedOffboardingAllocationStatus.Completed;
            case BatchedOffboardingAllocationStatus.InProgress:
                return next == BatchedOffboardingAllocationStatus.Completed;
            case BatchedOffboardingAllocationStatus.Completed:
                switch (next)
                {
                    case BatchedOffboardingAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedOffboardingAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched offboarding allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"batched offboarding allocation"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)BatchSize * Margin, 2);
    }

    public BatchedOffboardingAllocation Copy()
    {
        return new BatchedOffboardingAllocation(
            Code,
            Label,
            Hours,
            BatchSize,
            Approved,
            Margin,
            Timezone,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
