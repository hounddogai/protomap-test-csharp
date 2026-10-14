namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict policy ack claim is in its life cycle.</summary>
public enum StrictPolicyAckClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A strict policy ack claim as the policy ack team keeps it.</summary>
public sealed class StrictPolicyAckClaim(string code, string label, string channel, string skill, double minimum, double discount, int attempts, string summary, StrictPolicyAckClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public string Skill { get; set; } = skill;
    public double Minimum { get; set; } = minimum;
    public double Discount { get; set; } = discount;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public StrictPolicyAckClaimStatus Status { get; set; } = status;

    /// <summary>Starts a strict policy ack claim in status idle.</summary>
    public static StrictPolicyAckClaim NewStrictPolicyAckClaim(string code, string label)
    {
        return new StrictPolicyAckClaim(code, label, "", "", 0.0, 0.0, 0, "", StrictPolicyAckClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPolicyAckClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPolicyAckClaimStatus next)
    {
        switch (Status)
        {
            case StrictPolicyAckClaimStatus.Idle:
                return next == StrictPolicyAckClaimStatus.Ready || next == StrictPolicyAckClaimStatus.Draining;
            case StrictPolicyAckClaimStatus.Warming:
                return next == StrictPolicyAckClaimStatus.Ready;
            case StrictPolicyAckClaimStatus.Ready:
                switch (next)
                {
                    case StrictPolicyAckClaimStatus.Draining:
                    case StrictPolicyAckClaimStatus.Stopped:
                        return true;
                }
                return false;
            case StrictPolicyAckClaimStatus.Draining:
                return next == StrictPolicyAckClaimStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictPolicyAckClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict policy ack claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Discount * (double)Attempts, 2);
    }

    public StrictPolicyAckClaim Copy()
    {
        return new StrictPolicyAckClaim(Code, Label, Channel, Skill, Minimum, Discount, Attempts, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
