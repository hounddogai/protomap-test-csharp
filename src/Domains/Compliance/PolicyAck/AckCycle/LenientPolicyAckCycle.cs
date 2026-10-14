namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient policy ack cycle is in its life cycle.</summary>
public enum LenientPolicyAckCycleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient policy ack cycle as the policy ack team keeps it.</summary>
public sealed class LenientPolicyAckCycle(string code, string label, int sequence, string grade, bool visible, int attempts, string jobCode, string contactEmail, int createdAt, LenientPolicyAckCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Sequence { get; set; } = sequence;
    public string Grade { get; set; } = grade;
    public bool Visible { get; set; } = visible;
    public int Attempts { get; set; } = attempts;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public int CreatedAt { get; set; } = createdAt;
    public LenientPolicyAckCycleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient policy ack cycle in status open.</summary>
    public static LenientPolicyAckCycle NewLenientPolicyAckCycle(string code, string label)
    {
        return new LenientPolicyAckCycle(code, label, 0, "", false, 0, "", "", 0, LenientPolicyAckCycleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPolicyAckCycleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPolicyAckCycleStatus next)
    {
        switch (Status)
        {
            case LenientPolicyAckCycleStatus.Open:
                switch (next)
                {
                    case LenientPolicyAckCycleStatus.Resolved:
                        return true;
                }
                return false;
            case LenientPolicyAckCycleStatus.InReview:
                return next == LenientPolicyAckCycleStatus.Resolved || next == LenientPolicyAckCycleStatus.Archived;
            case LenientPolicyAckCycleStatus.Resolved:
                return next == LenientPolicyAckCycleStatus.Reopened || next == LenientPolicyAckCycleStatus.Archived;
            case LenientPolicyAckCycleStatus.Reopened:
                return next == LenientPolicyAckCycleStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LenientPolicyAckCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient policy ack cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)CreatedAt + (double)Attempts, 2);
    }

    public LenientPolicyAckCycle Copy()
    {
        return new LenientPolicyAckCycle(
            Code,
            Label,
            Sequence,
            Grade,
            Visible,
            Attempts,
            JobCode,
            ContactEmail,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
