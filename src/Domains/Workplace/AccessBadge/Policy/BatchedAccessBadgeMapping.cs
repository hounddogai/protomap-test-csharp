namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched access badge mapping is in its life cycle.</summary>
public enum BatchedAccessBadgeMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a batched access badge mapping and its current status.</summary>
public sealed class BatchedAccessBadgeMapping(string code, string label, string costCenter, string category, int batchSize, int leaveDays, BatchedAccessBadgeMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Category { get; set; } = category;
    public int BatchSize { get; set; } = batchSize;
    public int LeaveDays { get; set; } = leaveDays;
    public BatchedAccessBadgeMappingStatus Status { get; set; } = status;

    /// <summary>Starts a batched access badge mapping in status requested.</summary>
    public static BatchedAccessBadgeMapping NewBatchedAccessBadgeMapping(string code, string label)
    {
        return new BatchedAccessBadgeMapping(code, label, "", "", 0, 0, BatchedAccessBadgeMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAccessBadgeMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAccessBadgeMappingStatus next)
    {
        switch (Status)
        {
            case BatchedAccessBadgeMappingStatus.Requested:
                switch (next)
                {
                    case BatchedAccessBadgeMappingStatus.Granted:
                    case BatchedAccessBadgeMappingStatus.Expired:
                        return true;
                }
                return false;
            case BatchedAccessBadgeMappingStatus.Granted:
                switch (next)
                {
                    case BatchedAccessBadgeMappingStatus.Revoked:
                    case BatchedAccessBadgeMappingStatus.Expired:
                        return true;
                }
                return false;
            case BatchedAccessBadgeMappingStatus.Revoked:
                return next == BatchedAccessBadgeMappingStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BatchedAccessBadgeMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched access badge mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays + (double)BatchSize, 2);
    }

    public BatchedAccessBadgeMapping Copy()
    {
        return new BatchedAccessBadgeMapping(Code, Label, CostCenter, Category, BatchSize, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
