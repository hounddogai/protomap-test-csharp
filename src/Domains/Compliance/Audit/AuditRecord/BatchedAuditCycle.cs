namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched audit cycle is in its life cycle.</summary>
public enum BatchedAuditCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched audit cycle. Its code is unique within a tenant.</summary>
public sealed class BatchedAuditCycle(string code, string name, int tenureMonths, int graceDays, int leaveDays, int expiresAt, double cost, string country, bool archived, string employeeId, BatchedAuditCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public int GraceDays { get; set; } = graceDays;
    public int LeaveDays { get; set; } = leaveDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Cost { get; set; } = cost;
    public string Country { get; set; } = country;
    public bool Archived { get; set; } = archived;
    public string EmployeeId { get; set; } = employeeId;
    public BatchedAuditCycleStatus Status { get; set; } = status;

    /// <summary>Starts a batched audit cycle in status created.</summary>
    public static BatchedAuditCycle NewBatchedAuditCycle(string code, string name)
    {
        return new BatchedAuditCycle(code, name, 0, 0, 0, 0, 0.0, "", false, "", BatchedAuditCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAuditCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAuditCycleStatus next)
    {
        switch (Status)
        {
            case BatchedAuditCycleStatus.Created:
                switch (next)
                {
                    case BatchedAuditCycleStatus.Validated:
                        return true;
                }
                return false;
            case BatchedAuditCycleStatus.Validated:
                return next == BatchedAuditCycleStatus.Retired;
            case BatchedAuditCycleStatus.Published:
                return next == BatchedAuditCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedAuditCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched audit cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched audit cycle"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)ExpiresAt - (double)TenureMonths, 2);
    }

    public BatchedAuditCycle Copy()
    {
        return new BatchedAuditCycle(
            Code,
            Name,
            TenureMonths,
            GraceDays,
            LeaveDays,
            ExpiresAt,
            Cost,
            Country,
            Archived,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
