namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched access badge allocation is in its life cycle.</summary>
public enum BatchedAccessBadgeAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One batched access badge allocation, with what the planners need to act on it.</summary>
public sealed class BatchedAccessBadgeAllocation(string code, string title, string description, string checksum, double score, int graceDays, string externalRef, BatchedAccessBadgeAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Checksum { get; set; } = checksum;
    public double Score { get; set; } = score;
    public int GraceDays { get; set; } = graceDays;
    public string ExternalRef { get; set; } = externalRef;
    public BatchedAccessBadgeAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a batched access badge allocation in status open.</summary>
    public static BatchedAccessBadgeAllocation NewBatchedAccessBadgeAllocation(string code, string title)
    {
        return new BatchedAccessBadgeAllocation(
            code,
            title,
            "",
            "",
            0.0,
            0,
            "",
            BatchedAccessBadgeAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAccessBadgeAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAccessBadgeAllocationStatus next)
    {
        switch (Status)
        {
            case BatchedAccessBadgeAllocationStatus.Open:
                switch (next)
                {
                    case BatchedAccessBadgeAllocationStatus.Archived:
                        return true;
                }
                return false;
            case BatchedAccessBadgeAllocationStatus.InReview:
                switch (next)
                {
                    case BatchedAccessBadgeAllocationStatus.Resolved:
                    case BatchedAccessBadgeAllocationStatus.Archived:
                        return true;
                }
                return false;
            case BatchedAccessBadgeAllocationStatus.Resolved:
                return next == BatchedAccessBadgeAllocationStatus.Archived;
            case BatchedAccessBadgeAllocationStatus.Reopened:
                switch (next)
                {
                    case BatchedAccessBadgeAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAccessBadgeAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched access badge allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)GraceDays + Score, 2);
    }

    public BatchedAccessBadgeAllocation Copy()
    {
        return new BatchedAccessBadgeAllocation(
            Code,
            Title,
            Description,
            Checksum,
            Score,
            GraceDays,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
