namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive garnishment allocation is in its life cycle.</summary>
public enum AdaptiveGarnishmentAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive garnishment allocation. Its code is unique within a tenant.</summary>
public sealed class AdaptiveGarnishmentAllocation(string code, string title, string name, string category, string managerId, double amount, AdaptiveGarnishmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string ManagerId { get; set; } = managerId;
    public double Amount { get; set; } = amount;
    public AdaptiveGarnishmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive garnishment allocation in status idle.</summary>
    public static AdaptiveGarnishmentAllocation NewAdaptiveGarnishmentAllocation(string code, string title)
    {
        return new AdaptiveGarnishmentAllocation(
            code,
            title,
            "",
            "",
            "",
            0.0,
            AdaptiveGarnishmentAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveGarnishmentAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveGarnishmentAllocationStatus next)
    {
        switch (Status)
        {
            case AdaptiveGarnishmentAllocationStatus.Idle:
                switch (next)
                {
                    case AdaptiveGarnishmentAllocationStatus.Warming:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentAllocationStatus.Warming:
                return next == AdaptiveGarnishmentAllocationStatus.Stopped;
            case AdaptiveGarnishmentAllocationStatus.Ready:
                switch (next)
                {
                    case AdaptiveGarnishmentAllocationStatus.Draining:
                    case AdaptiveGarnishmentAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentAllocationStatus.Draining:
                return next == AdaptiveGarnishmentAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveGarnishmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive garnishment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public AdaptiveGarnishmentAllocation Copy()
    {
        return new AdaptiveGarnishmentAllocation(Code, Title, Name, Category, ManagerId, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
