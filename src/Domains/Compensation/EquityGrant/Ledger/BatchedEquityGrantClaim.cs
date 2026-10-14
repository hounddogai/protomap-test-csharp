namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched equity grant claim is in its life cycle.</summary>
public enum BatchedEquityGrantClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched equity grant claim as the equity grant team keeps it.</summary>
public sealed class BatchedEquityGrantClaim(string code, string title, string ownerId, string category, double floorValue, double maximum, string badge, int graceDays, int capacity, int leaveDays, string employeeId, BatchedEquityGrantClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string Category { get; set; } = category;
    public double FloorValue { get; set; } = floorValue;
    public double Maximum { get; set; } = maximum;
    public string Badge { get; set; } = badge;
    public int GraceDays { get; set; } = graceDays;
    public int Capacity { get; set; } = capacity;
    public int LeaveDays { get; set; } = leaveDays;
    public string EmployeeId { get; set; } = employeeId;
    public BatchedEquityGrantClaimStatus Status { get; set; } = status;

    /// <summary>Starts a batched equity grant claim in status new.</summary>
    public static BatchedEquityGrantClaim NewBatchedEquityGrantClaim(string code, string title)
    {
        return new BatchedEquityGrantClaim(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            "",
            0,
            0,
            0,
            "",
            BatchedEquityGrantClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedEquityGrantClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedEquityGrantClaimStatus next)
    {
        switch (Status)
        {
            case BatchedEquityGrantClaimStatus.New:
                return next == BatchedEquityGrantClaimStatus.Running || next == BatchedEquityGrantClaimStatus.Failed;
            case BatchedEquityGrantClaimStatus.Queued:
                return next == BatchedEquityGrantClaimStatus.Failed;
            case BatchedEquityGrantClaimStatus.Running:
                switch (next)
                {
                    case BatchedEquityGrantClaimStatus.Done:
                        return true;
                }
                return false;
            case BatchedEquityGrantClaimStatus.Done:
                return next == BatchedEquityGrantClaimStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedEquityGrantClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched equity grant claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)GraceDays * (double)Capacity, 2);
    }

    public BatchedEquityGrantClaim Copy()
    {
        return new BatchedEquityGrantClaim(
            Code,
            Title,
            OwnerId,
            Category,
            FloorValue,
            Maximum,
            Badge,
            GraceDays,
            Capacity,
            LeaveDays,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
