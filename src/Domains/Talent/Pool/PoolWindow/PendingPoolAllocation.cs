namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pool allocation is in its life cycle.</summary>
public enum PendingPoolAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pending pool allocation and its current status.</summary>
public sealed class PendingPoolAllocation(string code, string title, double threshold, string externalRef, string unit, string managerId, int revision, int attempts, string costCenter, string currency, bool pinned, PendingPoolAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string ExternalRef { get; set; } = externalRef;
    public string Unit { get; set; } = unit;
    public string ManagerId { get; set; } = managerId;
    public int Revision { get; set; } = revision;
    public int Attempts { get; set; } = attempts;
    public string CostCenter { get; set; } = costCenter;
    public string Currency { get; set; } = currency;
    public bool Pinned { get; set; } = pinned;
    public PendingPoolAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pending pool allocation in status created.</summary>
    public static PendingPoolAllocation NewPendingPoolAllocation(string code, string title)
    {
        return new PendingPoolAllocation(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0,
            0,
            "",
            "",
            false,
            PendingPoolAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPoolAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPoolAllocationStatus next)
    {
        switch (Status)
        {
            case PendingPoolAllocationStatus.Created:
                switch (next)
                {
                    case PendingPoolAllocationStatus.Validated:
                        return true;
                }
                return false;
            case PendingPoolAllocationStatus.Validated:
                return next == PendingPoolAllocationStatus.Published;
            case PendingPoolAllocationStatus.Published:
                switch (next)
                {
                    case PendingPoolAllocationStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPoolAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pool allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Revision * (double)Attempts, 2);
    }

    public PendingPoolAllocation Copy()
    {
        return new PendingPoolAllocation(
            Code,
            Title,
            Threshold,
            ExternalRef,
            Unit,
            ManagerId,
            Revision,
            Attempts,
            CostCenter,
            Currency,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
