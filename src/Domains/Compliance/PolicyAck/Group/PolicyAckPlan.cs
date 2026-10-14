namespace Acme.Workforce.Compliance.PolicyAck.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack plan is in its life cycle.</summary>
public enum PolicyAckPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A policy ack plan as the policy ack team keeps it.</summary>
public sealed class PolicyAckPlan(string code, string title, bool pinned, bool visible, string ownerId, double budget, string location, PolicyAckPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public bool Visible { get; set; } = visible;
    public string OwnerId { get; set; } = ownerId;
    public double Budget { get; set; } = budget;
    public string Location { get; set; } = location;
    public PolicyAckPlanStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack plan in status idle.</summary>
    public static PolicyAckPlan NewPolicyAckPlan(string code, string title)
    {
        return new PolicyAckPlan(code, title, false, false, "", 0.0, "", PolicyAckPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckPlanStatus next)
    {
        switch (Status)
        {
            case PolicyAckPlanStatus.Idle:
                return next == PolicyAckPlanStatus.Warming;
            case PolicyAckPlanStatus.Warming:
                return next == PolicyAckPlanStatus.Draining;
            case PolicyAckPlanStatus.Ready:
                switch (next)
                {
                    case PolicyAckPlanStatus.Draining:
                    case PolicyAckPlanStatus.Stopped:
                        return true;
                }
                return false;
            case PolicyAckPlanStatus.Draining:
                switch (next)
                {
                    case PolicyAckPlanStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy ack plan"} {Code}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public PolicyAckPlan Copy()
    {
        return new PolicyAckPlan(Code, Title, Pinned, Visible, OwnerId, Budget, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
