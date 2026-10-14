namespace Acme.Workforce.Compliance.PolicyAck.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack estimate is in its life cycle.</summary>
public enum PolicyAckEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A policy ack estimate as the policy ack team keeps it.</summary>
public sealed class PolicyAckEstimate(string code, string name, int priority, double budget, bool pinned, string region, PolicyAckEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Budget { get; set; } = budget;
    public bool Pinned { get; set; } = pinned;
    public string Region { get; set; } = region;
    public PolicyAckEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack estimate in status draft.</summary>
    public static PolicyAckEstimate NewPolicyAckEstimate(string code, string name)
    {
        return new PolicyAckEstimate(code, name, 0, 0.0, false, "", PolicyAckEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckEstimateStatus next)
    {
        switch (Status)
        {
            case PolicyAckEstimateStatus.Draft:
                return next == PolicyAckEstimateStatus.Pending;
            case PolicyAckEstimateStatus.Pending:
                switch (next)
                {
                    case PolicyAckEstimateStatus.Active:
                    case PolicyAckEstimateStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAckEstimateStatus.Active:
                return next == PolicyAckEstimateStatus.Suspended || next == PolicyAckEstimateStatus.Closed;
            case PolicyAckEstimateStatus.Suspended:
                return next == PolicyAckEstimateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PolicyAckEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Priority * Budget, 2);
    }

    public PolicyAckEstimate Copy()
    {
        return new PolicyAckEstimate(Code, Name, Priority, Budget, Pinned, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
