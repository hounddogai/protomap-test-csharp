namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy milestone is in its life cycle.</summary>
public enum PolicyMilestoneStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A policy milestone as the policy team keeps it.</summary>
public sealed class PolicyMilestone(string code, string title, bool pinned, bool archived, string contactEmail, double amount, PolicyMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public bool Archived { get; set; } = archived;
    public string ContactEmail { get; set; } = contactEmail;
    public double Amount { get; set; } = amount;
    public PolicyMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a policy milestone in status requested.</summary>
    public static PolicyMilestone NewPolicyMilestone(string code, string title)
    {
        return new PolicyMilestone(code, title, false, false, "", 0.0, PolicyMilestoneStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyMilestoneStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyMilestoneStatus next)
    {
        switch (Status)
        {
            case PolicyMilestoneStatus.Requested:
                return next == PolicyMilestoneStatus.Expired;
            case PolicyMilestoneStatus.Granted:
                return next == PolicyMilestoneStatus.Revoked;
            case PolicyMilestoneStatus.Revoked:
                switch (next)
                {
                    case PolicyMilestoneStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PolicyMilestone Copy()
    {
        return new PolicyMilestone(Code, Title, Pinned, Archived, ContactEmail, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
