namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched span allocation is in its life cycle.</summary>
public enum BatchedSpanAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a batched span allocation and its current status.</summary>
public sealed class BatchedSpanAllocation(string code, string label, int createdAt, string unit, string grade, string contactEmail, BatchedSpanAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string Unit { get; set; } = unit;
    public string Grade { get; set; } = grade;
    public string ContactEmail { get; set; } = contactEmail;
    public BatchedSpanAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a batched span allocation in status created.</summary>
    public static BatchedSpanAllocation NewBatchedSpanAllocation(string code, string label)
    {
        return new BatchedSpanAllocation(code, label, 0, "", "", "", BatchedSpanAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSpanAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSpanAllocationStatus next)
    {
        switch (Status)
        {
            case BatchedSpanAllocationStatus.Created:
                return next == BatchedSpanAllocationStatus.Retired;
            case BatchedSpanAllocationStatus.Validated:
                return next == BatchedSpanAllocationStatus.Published || next == BatchedSpanAllocationStatus.Retired;
            case BatchedSpanAllocationStatus.Published:
                switch (next)
                {
                    case BatchedSpanAllocationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedSpanAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched span allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public BatchedSpanAllocation Copy()
    {
        return new BatchedSpanAllocation(Code, Label, CreatedAt, Unit, Grade, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
