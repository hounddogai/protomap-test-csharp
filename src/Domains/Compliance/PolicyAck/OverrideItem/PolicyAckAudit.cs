namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack audit is in its life cycle.</summary>
public enum PolicyAckAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a policy ack audit and its current status.</summary>
public sealed class PolicyAckAudit(string code, string label, double discount, string employeeId, string summary, int windowDays, string category, double floorValue, double threshold, PolicyAckAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string EmployeeId { get; set; } = employeeId;
    public string Summary { get; set; } = summary;
    public int WindowDays { get; set; } = windowDays;
    public string Category { get; set; } = category;
    public double FloorValue { get; set; } = floorValue;
    public double Threshold { get; set; } = threshold;
    public PolicyAckAuditStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack audit in status idle.</summary>
    public static PolicyAckAudit NewPolicyAckAudit(string code, string label)
    {
        return new PolicyAckAudit(code, label, 0.0, "", "", 0, "", 0.0, 0.0, PolicyAckAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckAuditStatus next)
    {
        switch (Status)
        {
            case PolicyAckAuditStatus.Idle:
                switch (next)
                {
                    case PolicyAckAuditStatus.Draining:
                    case PolicyAckAuditStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyAckAuditStatus.Warming:
                return next == PolicyAckAuditStatus.Draining || next == PolicyAckAuditStatus.Stopped;
            case PolicyAckAuditStatus.Ready:
                switch (next)
                {
                    case PolicyAckAuditStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyAckAuditStatus.Draining:
                return next == PolicyAckAuditStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PolicyAckAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Threshold - (double)WindowDays, 2);
    }

    public PolicyAckAudit Copy()
    {
        return new PolicyAckAudit(
            Code,
            Label,
            Discount,
            EmployeeId,
            Summary,
            WindowDays,
            Category,
            FloorValue,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
