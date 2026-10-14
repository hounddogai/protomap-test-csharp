namespace Acme.Workforce.Workplace.Occupancy.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched occupancy mapping is in its life cycle.</summary>
public enum BatchedOccupancyMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A batched occupancy mapping. Its code is unique within a tenant.</summary>
public sealed class BatchedOccupancyMapping(string code, string name, bool approved, string externalRef, int limit, string reviewer, double discount, bool active, double rating, BatchedOccupancyMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public string ExternalRef { get; set; } = externalRef;
    public int Limit { get; set; } = limit;
    public string Reviewer { get; set; } = reviewer;
    public double Discount { get; set; } = discount;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public BatchedOccupancyMappingStatus Status { get; set; } = status;

    /// <summary>Starts a batched occupancy mapping in status open.</summary>
    public static BatchedOccupancyMapping NewBatchedOccupancyMapping(string code, string name)
    {
        return new BatchedOccupancyMapping(
            code,
            name,
            false,
            "",
            0,
            "",
            0.0,
            false,
            0.0,
            BatchedOccupancyMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedOccupancyMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedOccupancyMappingStatus next)
    {
        switch (Status)
        {
            case BatchedOccupancyMappingStatus.Open:
                switch (next)
                {
                    case BatchedOccupancyMappingStatus.Resolved:
                        return true;
                }
                return false;
            case BatchedOccupancyMappingStatus.InReview:
                return next == BatchedOccupancyMappingStatus.Resolved;
            case BatchedOccupancyMappingStatus.Resolved:
                switch (next)
                {
                    case BatchedOccupancyMappingStatus.Reopened:
                    case BatchedOccupancyMappingStatus.Archived:
                        return true;
                }
                return false;
            case BatchedOccupancyMappingStatus.Reopened:
                return next == BatchedOccupancyMappingStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BatchedOccupancyMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched occupancy mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched occupancy mapping"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rating * Discount, 2);
    }

    public BatchedOccupancyMapping Copy()
    {
        return new BatchedOccupancyMapping(
            Code,
            Name,
            Approved,
            ExternalRef,
            Limit,
            Reviewer,
            Discount,
            Active,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
