namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack milestone is in its life cycle.</summary>
public enum PolicyAckMilestoneStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A policy ack milestone. Its code is unique within a tenant.</summary>
public sealed class PolicyAckMilestone(string code, string name, int sequence, string department, string employeeId, string title, int capacity, string checksum, double minimum, string costCenter, PolicyAckMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Sequence { get; set; } = sequence;
    public string Department { get; set; } = department;
    public string EmployeeId { get; set; } = employeeId;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Checksum { get; set; } = checksum;
    public double Minimum { get; set; } = minimum;
    public string CostCenter { get; set; } = costCenter;
    public PolicyAckMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack milestone in status created.</summary>
    public static PolicyAckMilestone NewPolicyAckMilestone(string code, string name)
    {
        return new PolicyAckMilestone(code, name, 0, "", "", "", 0, "", 0.0, "", PolicyAckMilestoneStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckMilestoneStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckMilestoneStatus next)
    {
        switch (Status)
        {
            case PolicyAckMilestoneStatus.Created:
                return next == PolicyAckMilestoneStatus.Published;
            case PolicyAckMilestoneStatus.Validated:
                switch (next)
                {
                    case PolicyAckMilestoneStatus.Retired:
                        return true;
                }
                return false;
            case PolicyAckMilestoneStatus.Published:
                switch (next)
                {
                    case PolicyAckMilestoneStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy ack milestone"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Sequence * Minimum, 2);
    }

    public PolicyAckMilestone Copy()
    {
        return new PolicyAckMilestone(
            Code,
            Name,
            Sequence,
            Department,
            EmployeeId,
            Title,
            Capacity,
            Checksum,
            Minimum,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
