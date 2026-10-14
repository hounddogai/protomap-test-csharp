namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback attestation agreement is in its life cycle.</summary>
public enum FallbackAttestationAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a fallback attestation agreement and its current status.</summary>
public sealed class FallbackAttestationAgreement(string code, string name, string segment, double score, int expiresAt, string description, FallbackAttestationAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Description { get; set; } = description;
    public FallbackAttestationAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a fallback attestation agreement in status requested.</summary>
    public static FallbackAttestationAgreement NewFallbackAttestationAgreement(string code, string name)
    {
        return new FallbackAttestationAgreement(
            code,
            name,
            "",
            0.0,
            0,
            "",
            FallbackAttestationAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAttestationAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAttestationAgreementStatus next)
    {
        switch (Status)
        {
            case FallbackAttestationAgreementStatus.Requested:
                return next == FallbackAttestationAgreementStatus.Granted || next == FallbackAttestationAgreementStatus.Expired;
            case FallbackAttestationAgreementStatus.Granted:
                return next == FallbackAttestationAgreementStatus.Revoked;
            case FallbackAttestationAgreementStatus.Revoked:
                return next == FallbackAttestationAgreementStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FallbackAttestationAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback attestation agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback attestation agreement"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)ExpiresAt * Score, 2);
    }

    public FallbackAttestationAgreement Copy()
    {
        return new FallbackAttestationAgreement(Code, Name, Segment, Score, ExpiresAt, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
