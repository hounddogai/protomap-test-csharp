namespace Acme.Workforce.Talent.Pool.PoolDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched pool audit is in its life cycle.</summary>
public enum BatchedPoolAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched pool audit as the pool team keeps it.</summary>
public sealed class BatchedPoolAudit(string code, string name, int limit, string costCenter, int capacity, BatchedPoolAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string CostCenter { get; set; } = costCenter;
    public int Capacity { get; set; } = capacity;
    public BatchedPoolAuditStatus Status { get; set; } = status;

    /// <summary>Starts a batched pool audit in status created.</summary>
    public static BatchedPoolAudit NewBatchedPoolAudit(string code, string name)
    {
        return new BatchedPoolAudit(code, name, 0, "", 0, BatchedPoolAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPoolAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPoolAuditStatus next)
    {
        switch (Status)
        {
            case BatchedPoolAuditStatus.Created:
                return next == BatchedPoolAuditStatus.Retired;
            case BatchedPoolAuditStatus.Validated:
                return next == BatchedPoolAuditStatus.Retired;
            case BatchedPoolAuditStatus.Published:
                switch (next)
                {
                    case BatchedPoolAuditStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPoolAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched pool audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched pool audit"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Capacity * (double)Limit, 2);
    }

    public BatchedPoolAudit Copy()
    {
        return new BatchedPoolAudit(Code, Name, Limit, CostCenter, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
