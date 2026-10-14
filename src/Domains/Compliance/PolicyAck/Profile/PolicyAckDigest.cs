namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack digest is in its life cycle.</summary>
public enum PolicyAckDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A policy ack digest as the policy ack team keeps it.</summary>
public sealed class PolicyAckDigest(string code, string title, bool archived, string skill, bool visible, List<string> tags, int capacity, double floorValue, double ratio, PolicyAckDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Skill { get; set; } = skill;
    public bool Visible { get; set; } = visible;
    public List<string> Tags { get; set; } = tags;
    public int Capacity { get; set; } = capacity;
    public double FloorValue { get; set; } = floorValue;
    public double Ratio { get; set; } = ratio;
    public PolicyAckDigestStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack digest in status planned.</summary>
    public static PolicyAckDigest NewPolicyAckDigest(string code, string title)
    {
        return new PolicyAckDigest(
            code,
            title,
            false,
            "",
            false,
            new List<string>(),
            0,
            0.0,
            0.0,
            PolicyAckDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckDigestStatus next)
    {
        switch (Status)
        {
            case PolicyAckDigestStatus.Planned:
                switch (next)
                {
                    case PolicyAckDigestStatus.Scheduled:
                    case PolicyAckDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckDigestStatus.Scheduled:
                return next == PolicyAckDigestStatus.Completed;
            case PolicyAckDigestStatus.InProgress:
                switch (next)
                {
                    case PolicyAckDigestStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckDigestStatus.Completed:
                return next == PolicyAckDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PolicyAckDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedTotal()
    {
        return Math.Round(FloorValue + (double)Capacity, 2);
    }

    public PolicyAckDigest Copy()
    {
        return new PolicyAckDigest(Code, Title, Archived, Skill, Visible, Tags, Capacity, FloorValue, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
