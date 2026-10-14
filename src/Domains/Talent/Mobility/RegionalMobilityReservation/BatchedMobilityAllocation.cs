namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched mobility allocation is in its life cycle.</summary>
public enum BatchedMobilityAllocationStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A batched mobility allocation. Its code is unique within a tenant.</summary>
public sealed class BatchedMobilityAllocation(string code, string title, string externalRef, int sequence, int updatedAt, string location, string salaryBand, string description, string segment, Dictionary<string, string> attributes, int priority, BatchedMobilityAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public int Sequence { get; set; } = sequence;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Location { get; set; } = location;
    public string SalaryBand { get; set; } = salaryBand;
    public string Description { get; set; } = description;
    public string Segment { get; set; } = segment;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Priority { get; set; } = priority;
    public BatchedMobilityAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a batched mobility allocation in status draft.</summary>
    public static BatchedMobilityAllocation NewBatchedMobilityAllocation(string code, string title)
    {
        return new BatchedMobilityAllocation(
            code,
            title,
            "",
            0,
            0,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0,
            BatchedMobilityAllocationStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedMobilityAllocationStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedMobilityAllocationStatus next)
    {
        switch (Status)
        {
            case BatchedMobilityAllocationStatus.Draft:
                switch (next)
                {
                    case BatchedMobilityAllocationStatus.Suspended:
                        return true;
                }
                return false;
            case BatchedMobilityAllocationStatus.Pending:
                return next == BatchedMobilityAllocationStatus.Suspended || next == BatchedMobilityAllocationStatus.Closed;
            case BatchedMobilityAllocationStatus.Active:
                return next == BatchedMobilityAllocationStatus.Suspended || next == BatchedMobilityAllocationStatus.Closed;
            case BatchedMobilityAllocationStatus.Suspended:
                return next == BatchedMobilityAllocationStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BatchedMobilityAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched mobility allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Priority + (double)Sequence, 2);
    }

    public BatchedMobilityAllocation Copy()
    {
        return new BatchedMobilityAllocation(
            Code,
            Title,
            ExternalRef,
            Sequence,
            UpdatedAt,
            Location,
            SalaryBand,
            Description,
            Segment,
            Attributes,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
