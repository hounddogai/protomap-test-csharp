namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient policy ack milestone is in its life cycle.</summary>
public enum LenientPolicyAckMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient policy ack milestone. Its code is unique within a tenant.</summary>
public sealed class LenientPolicyAckMilestone(string code, string label, double fte, int tenureMonths, bool pinned, string checksum, LenientPolicyAckMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Pinned { get; set; } = pinned;
    public string Checksum { get; set; } = checksum;
    public LenientPolicyAckMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a lenient policy ack milestone in status draft.</summary>
    public static LenientPolicyAckMilestone NewLenientPolicyAckMilestone(string code, string label)
    {
        return new LenientPolicyAckMilestone(code, label, 0.0, 0, false, "", LenientPolicyAckMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPolicyAckMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPolicyAckMilestoneStatus next)
    {
        switch (Status)
        {
            case LenientPolicyAckMilestoneStatus.Draft:
                switch (next)
                {
                    case LenientPolicyAckMilestoneStatus.Pending:
                    case LenientPolicyAckMilestoneStatus.Closed:
                        return true;
                }
                return false;
            case LenientPolicyAckMilestoneStatus.Pending:
                return next == LenientPolicyAckMilestoneStatus.Closed;
            case LenientPolicyAckMilestoneStatus.Active:
                return next == LenientPolicyAckMilestoneStatus.Suspended || next == LenientPolicyAckMilestoneStatus.Closed;
            case LenientPolicyAckMilestoneStatus.Suspended:
                switch (next)
                {
                    case LenientPolicyAckMilestoneStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPolicyAckMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient policy ack milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)TenureMonths - Fte, 2);
    }

    public LenientPolicyAckMilestone Copy()
    {
        return new LenientPolicyAckMilestone(Code, Label, Fte, TenureMonths, Pinned, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
