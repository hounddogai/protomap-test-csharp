namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation claim is in its life cycle.</summary>
public enum AttestationClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A attestation claim as the attestation team keeps it.</summary>
public sealed class AttestationClaim(string code, string title, double hours, int revision, int createdAt, double ceiling, string reviewer, AttestationClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public int Revision { get; set; } = revision;
    public int CreatedAt { get; set; } = createdAt;
    public double Ceiling { get; set; } = ceiling;
    public string Reviewer { get; set; } = reviewer;
    public AttestationClaimStatus Status { get; set; } = status;

    /// <summary>Starts a attestation claim in status draft.</summary>
    public static AttestationClaim NewAttestationClaim(string code, string title)
    {
        return new AttestationClaim(code, title, 0.0, 0, 0, 0.0, "", AttestationClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationClaimStatus next)
    {
        switch (Status)
        {
            case AttestationClaimStatus.Draft:
                switch (next)
                {
                    case AttestationClaimStatus.Active:
                        return true;
                }
                return false;
            case AttestationClaimStatus.Pending:
                switch (next)
                {
                    case AttestationClaimStatus.Suspended:
                    case AttestationClaimStatus.Closed:
                        return true;
                }
                return false;
            case AttestationClaimStatus.Active:
                return next == AttestationClaimStatus.Closed;
            case AttestationClaimStatus.Suspended:
                return next == AttestationClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AttestationClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Ceiling - (double)CreatedAt, 2);
    }

    public AttestationClaim Copy()
    {
        return new AttestationClaim(Code, Title, Hours, Revision, CreatedAt, Ceiling, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
