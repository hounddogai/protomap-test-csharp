namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary policy ack draft is in its life cycle.</summary>
public enum PrimaryPolicyAckDraftStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary policy ack draft. Its code is unique within a tenant.</summary>
public sealed class PrimaryPolicyAckDraft(string code, string title, double ceiling, int priority, int sequence, PrimaryPolicyAckDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public int Priority { get; set; } = priority;
    public int Sequence { get; set; } = sequence;
    public PrimaryPolicyAckDraftStatus Status { get; set; } = status;

    /// <summary>Starts a primary policy ack draft in status open.</summary>
    public static PrimaryPolicyAckDraft NewPrimaryPolicyAckDraft(string code, string title)
    {
        return new PrimaryPolicyAckDraft(code, title, 0.0, 0, 0, PrimaryPolicyAckDraftStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPolicyAckDraftStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPolicyAckDraftStatus next)
    {
        switch (Status)
        {
            case PrimaryPolicyAckDraftStatus.Open:
                return next == PrimaryPolicyAckDraftStatus.Reopened || next == PrimaryPolicyAckDraftStatus.Archived;
            case PrimaryPolicyAckDraftStatus.InReview:
                return next == PrimaryPolicyAckDraftStatus.Reopened || next == PrimaryPolicyAckDraftStatus.Archived;
            case PrimaryPolicyAckDraftStatus.Resolved:
                return next == PrimaryPolicyAckDraftStatus.Reopened;
            case PrimaryPolicyAckDraftStatus.Reopened:
                return next == PrimaryPolicyAckDraftStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryPolicyAckDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary policy ack draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary policy ack draft"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Sequence * Ceiling, 2);
    }

    public PrimaryPolicyAckDraft Copy()
    {
        return new PrimaryPolicyAckDraft(Code, Title, Ceiling, Priority, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
