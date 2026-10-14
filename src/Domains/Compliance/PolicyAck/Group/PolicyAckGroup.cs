namespace Acme.Workforce.Compliance.PolicyAck.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack group is in its life cycle.</summary>
public enum PolicyAckGroupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One policy ack group, with what the planners need to act on it.</summary>
public sealed class PolicyAckGroup(string code, string title, double score, string segment, int version, int tenureMonths, int quantity, int sequence, int leaveDays, bool active, PolicyAckGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Segment { get; set; } = segment;
    public int Version { get; set; } = version;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Quantity { get; set; } = quantity;
    public int Sequence { get; set; } = sequence;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Active { get; set; } = active;
    public PolicyAckGroupStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack group in status proposed.</summary>
    public static PolicyAckGroup NewPolicyAckGroup(string code, string title)
    {
        return new PolicyAckGroup(code, title, 0.0, "", 0, 0, 0, 0, 0, false, PolicyAckGroupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckGroupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckGroupStatus next)
    {
        switch (Status)
        {
            case PolicyAckGroupStatus.Proposed:
                switch (next)
                {
                    case PolicyAckGroupStatus.Rejected:
                        return true;
                }
                return false;
            case PolicyAckGroupStatus.Approved:
                return next == PolicyAckGroupStatus.Withdrawn;
            case PolicyAckGroupStatus.Rejected:
                return next == PolicyAckGroupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PolicyAckGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)TenureMonths * Score, 2);
    }

    public PolicyAckGroup Copy()
    {
        return new PolicyAckGroup(
            Code,
            Title,
            Score,
            Segment,
            Version,
            TenureMonths,
            Quantity,
            Sequence,
            LeaveDays,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
