namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack allocation is in its life cycle.</summary>
public enum PolicyAckAllocationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A policy ack allocation. Its code is unique within a tenant.</summary>
public sealed class PolicyAckAllocation(string code, string label, int version, string grade, int limit, string ownerId, int tenureMonths, double cost, string location, bool pinned, PolicyAckAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Grade { get; set; } = grade;
    public int Limit { get; set; } = limit;
    public string OwnerId { get; set; } = ownerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Cost { get; set; } = cost;
    public string Location { get; set; } = location;
    public bool Pinned { get; set; } = pinned;
    public PolicyAckAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack allocation in status proposed.</summary>
    public static PolicyAckAllocation NewPolicyAckAllocation(string code, string label)
    {
        return new PolicyAckAllocation(
            code,
            label,
            0,
            "",
            0,
            "",
            0,
            0.0,
            "",
            false,
            PolicyAckAllocationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckAllocationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckAllocationStatus next)
    {
        switch (Status)
        {
            case PolicyAckAllocationStatus.Proposed:
                return next == PolicyAckAllocationStatus.Approved || next == PolicyAckAllocationStatus.Rejected;
            case PolicyAckAllocationStatus.Approved:
                return next == PolicyAckAllocationStatus.Rejected || next == PolicyAckAllocationStatus.Withdrawn;
            case PolicyAckAllocationStatus.Rejected:
                return next == PolicyAckAllocationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyAckAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Cost - (double)TenureMonths, 2);
    }

    public PolicyAckAllocation Copy()
    {
        return new PolicyAckAllocation(
            Code,
            Label,
            Version,
            Grade,
            Limit,
            OwnerId,
            TenureMonths,
            Cost,
            Location,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
