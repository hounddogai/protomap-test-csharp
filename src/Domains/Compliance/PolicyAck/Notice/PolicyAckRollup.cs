namespace Acme.Workforce.Compliance.PolicyAck.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack rollup is in its life cycle.</summary>
public enum PolicyAckRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a policy ack rollup and its current status.</summary>
public sealed class PolicyAckRollup(string code, string name, int tenureMonths, int leaveDays, double threshold, double floorValue, PolicyAckRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public int LeaveDays { get; set; } = leaveDays;
    public double Threshold { get; set; } = threshold;
    public double FloorValue { get; set; } = floorValue;
    public PolicyAckRollupStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack rollup in status planned.</summary>
    public static PolicyAckRollup NewPolicyAckRollup(string code, string name)
    {
        return new PolicyAckRollup(code, name, 0, 0, 0.0, 0.0, PolicyAckRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckRollupStatus next)
    {
        switch (Status)
        {
            case PolicyAckRollupStatus.Planned:
                switch (next)
                {
                    case PolicyAckRollupStatus.Scheduled:
                    case PolicyAckRollupStatus.Completed:
                        return true;
                }
                return false;
            case PolicyAckRollupStatus.Scheduled:
                switch (next)
                {
                    case PolicyAckRollupStatus.InProgress:
                        return true;
                }
                return false;
            case PolicyAckRollupStatus.InProgress:
                switch (next)
                {
                    case PolicyAckRollupStatus.Completed:
                    case PolicyAckRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckRollupStatus.Completed:
                switch (next)
                {
                    case PolicyAckRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)TenureMonths * Threshold, 2);
    }

    public PolicyAckRollup Copy()
    {
        return new PolicyAckRollup(Code, Name, TenureMonths, LeaveDays, Threshold, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
