namespace Acme.Workforce.Contractors.Offboarding.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding allocation is in its life cycle.</summary>
public enum OffboardingAllocationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A offboarding allocation. Its code is unique within a tenant.</summary>
public sealed class OffboardingAllocation(string code, string name, string summary, int priority, double amount, double rate, string source, OffboardingAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public int Priority { get; set; } = priority;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public string Source { get; set; } = source;
    public OffboardingAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding allocation in status proposed.</summary>
    public static OffboardingAllocation NewOffboardingAllocation(string code, string name)
    {
        return new OffboardingAllocation(code, name, "", 0, 0.0, 0.0, "", OffboardingAllocationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingAllocationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingAllocationStatus next)
    {
        switch (Status)
        {
            case OffboardingAllocationStatus.Proposed:
                switch (next)
                {
                    case OffboardingAllocationStatus.Rejected:
                        return true;
                }
                return false;
            case OffboardingAllocationStatus.Approved:
                return next == OffboardingAllocationStatus.Rejected || next == OffboardingAllocationStatus.Withdrawn;
            case OffboardingAllocationStatus.Rejected:
                return next == OffboardingAllocationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(OffboardingAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"offboarding allocation"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Amount + (double)Priority, 2);
    }

    public OffboardingAllocation Copy()
    {
        return new OffboardingAllocation(Code, Name, Summary, Priority, Amount, Rate, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
