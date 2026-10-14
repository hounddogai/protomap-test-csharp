namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation tier is in its life cycle.</summary>
public enum AttestationTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One attestation tier, with what the planners need to act on it.</summary>
public sealed class AttestationTier(string code, string label, double score, bool archived, int windowDays, int limit, int sequence, string reviewer, double amount, AttestationTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public int WindowDays { get; set; } = windowDays;
    public int Limit { get; set; } = limit;
    public int Sequence { get; set; } = sequence;
    public string Reviewer { get; set; } = reviewer;
    public double Amount { get; set; } = amount;
    public AttestationTierStatus Status { get; set; } = status;

    /// <summary>Starts a attestation tier in status open.</summary>
    public static AttestationTier NewAttestationTier(string code, string label)
    {
        return new AttestationTier(code, label, 0.0, false, 0, 0, 0, "", 0.0, AttestationTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationTierStatus next)
    {
        switch (Status)
        {
            case AttestationTierStatus.Open:
                switch (next)
                {
                    case AttestationTierStatus.Archived:
                        return true;
                }
                return false;
            case AttestationTierStatus.InReview:
                return next == AttestationTierStatus.Resolved;
            case AttestationTierStatus.Resolved:
                return next == AttestationTierStatus.Archived;
            case AttestationTierStatus.Reopened:
                return next == AttestationTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AttestationTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"attestation tier"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)WindowDays + Amount, 2);
    }

    public AttestationTier Copy()
    {
        return new AttestationTier(
            Code,
            Label,
            Score,
            Archived,
            WindowDays,
            Limit,
            Sequence,
            Reviewer,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
