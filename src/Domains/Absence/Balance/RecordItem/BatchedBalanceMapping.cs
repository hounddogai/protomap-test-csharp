namespace Acme.Workforce.Absence.Balance.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched balance mapping is in its life cycle.</summary>
public enum BatchedBalanceMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched balance mapping. Its code is unique within a tenant.</summary>
public sealed class BatchedBalanceMapping(string code, string name, string employeeId, bool visible, string jobCode, bool active, int version, string externalRef, BatchedBalanceMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public bool Visible { get; set; } = visible;
    public string JobCode { get; set; } = jobCode;
    public bool Active { get; set; } = active;
    public int Version { get; set; } = version;
    public string ExternalRef { get; set; } = externalRef;
    public BatchedBalanceMappingStatus Status { get; set; } = status;

    /// <summary>Starts a batched balance mapping in status proposed.</summary>
    public static BatchedBalanceMapping NewBatchedBalanceMapping(string code, string name)
    {
        return new BatchedBalanceMapping(
            code,
            name,
            "",
            false,
            "",
            false,
            0,
            "",
            BatchedBalanceMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedBalanceMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedBalanceMappingStatus next)
    {
        switch (Status)
        {
            case BatchedBalanceMappingStatus.Proposed:
                switch (next)
                {
                    case BatchedBalanceMappingStatus.Rejected:
                        return true;
                }
                return false;
            case BatchedBalanceMappingStatus.Approved:
                return next == BatchedBalanceMappingStatus.Rejected;
            case BatchedBalanceMappingStatus.Rejected:
                return next == BatchedBalanceMappingStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedBalanceMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched balance mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public BatchedBalanceMapping Copy()
    {
        return new BatchedBalanceMapping(
            Code,
            Name,
            EmployeeId,
            Visible,
            JobCode,
            Active,
            Version,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
