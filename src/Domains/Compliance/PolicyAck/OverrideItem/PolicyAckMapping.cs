namespace Acme.Workforce.Compliance.PolicyAck.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack mapping is in its life cycle.</summary>
public enum PolicyAckMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A policy ack mapping. Its code is unique within a tenant.</summary>
public sealed class PolicyAckMapping(string code, string title, double floorValue, int tenureMonths, bool approved, string category, PolicyAckMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Approved { get; set; } = approved;
    public string Category { get; set; } = category;
    public PolicyAckMappingStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack mapping in status planned.</summary>
    public static PolicyAckMapping NewPolicyAckMapping(string code, string title)
    {
        return new PolicyAckMapping(code, title, 0.0, 0, false, "", PolicyAckMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckMappingStatus next)
    {
        switch (Status)
        {
            case PolicyAckMappingStatus.Planned:
                return next == PolicyAckMappingStatus.Completed;
            case PolicyAckMappingStatus.Scheduled:
                switch (next)
                {
                    case PolicyAckMappingStatus.Completed:
                    case PolicyAckMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckMappingStatus.InProgress:
                switch (next)
                {
                    case PolicyAckMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckMappingStatus.Completed:
                return next == PolicyAckMappingStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PolicyAckMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(FloorValue - (double)TenureMonths, 2);
    }

    public PolicyAckMapping Copy()
    {
        return new PolicyAckMapping(Code, Title, FloorValue, TenureMonths, Approved, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
