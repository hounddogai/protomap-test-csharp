namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global policy allocation is in its life cycle.</summary>
public enum GlobalPolicyAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a global policy allocation and its current status.</summary>
public sealed class GlobalPolicyAllocation(string code, string name, string reviewer, double amount, int retries, string ownerId, GlobalPolicyAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public double Amount { get; set; } = amount;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public GlobalPolicyAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a global policy allocation in status idle.</summary>
    public static GlobalPolicyAllocation NewGlobalPolicyAllocation(string code, string name)
    {
        return new GlobalPolicyAllocation(code, name, "", 0.0, 0, "", GlobalPolicyAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPolicyAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPolicyAllocationStatus next)
    {
        switch (Status)
        {
            case GlobalPolicyAllocationStatus.Idle:
                switch (next)
                {
                    case GlobalPolicyAllocationStatus.Warming:
                    case GlobalPolicyAllocationStatus.Draining:
                        return true;
                }
                return false;
            case GlobalPolicyAllocationStatus.Warming:
                return next == GlobalPolicyAllocationStatus.Stopped;
            case GlobalPolicyAllocationStatus.Ready:
                switch (next)
                {
                    case GlobalPolicyAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalPolicyAllocationStatus.Draining:
                return next == GlobalPolicyAllocationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalPolicyAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global policy allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Amount - (double)Retries, 2);
    }

    public GlobalPolicyAllocation Copy()
    {
        return new GlobalPolicyAllocation(Code, Name, Reviewer, Amount, Retries, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
