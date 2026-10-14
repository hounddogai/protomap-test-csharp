namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending warranty allocation is in its life cycle.</summary>
public enum PendingWarrantyAllocationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pending warranty allocation as the warranty team keeps it.</summary>
public sealed class PendingWarrantyAllocation(string code, string title, string jobCode, string region, string employeeId, string source, double amount, PendingWarrantyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public string Region { get; set; } = region;
    public string EmployeeId { get; set; } = employeeId;
    public string Source { get; set; } = source;
    public double Amount { get; set; } = amount;
    public PendingWarrantyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pending warranty allocation in status proposed.</summary>
    public static PendingWarrantyAllocation NewPendingWarrantyAllocation(string code, string title)
    {
        return new PendingWarrantyAllocation(
            code,
            title,
            "",
            "",
            "",
            "",
            0.0,
            PendingWarrantyAllocationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingWarrantyAllocationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingWarrantyAllocationStatus next)
    {
        switch (Status)
        {
            case PendingWarrantyAllocationStatus.Proposed:
                return next == PendingWarrantyAllocationStatus.Rejected;
            case PendingWarrantyAllocationStatus.Approved:
                return next == PendingWarrantyAllocationStatus.Withdrawn;
            case PendingWarrantyAllocationStatus.Rejected:
                return next == PendingWarrantyAllocationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PendingWarrantyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending warranty allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PendingWarrantyAllocation Copy()
    {
        return new PendingWarrantyAllocation(Code, Title, JobCode, Region, EmployeeId, Source, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
