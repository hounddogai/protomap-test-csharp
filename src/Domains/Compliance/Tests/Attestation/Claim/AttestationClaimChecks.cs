namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationClaimChecks
{
    public static void AttestationClaimStartsInDraft()
    {
        var item = AttestationClaim.NewAttestationClaim("code-611", "title-436");
        Check.Equal(item.Status, AttestationClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AttestationClaimMovesToActive()
    {
        var item = AttestationClaim.NewAttestationClaim("code-611", "title-436");
        try
        {
            item.MoveTo(AttestationClaimStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AttestationClaimStatus.Active, "status");
    }

    public static void AttestationClaimRejectsMovesFromClosed()
    {
        var item = AttestationClaim.NewAttestationClaim("code-611", "title-436");
        item.Status = AttestationClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(AttestationClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AttestationClaimStartsInDraft();
        AttestationClaimMovesToActive();
        AttestationClaimRejectsMovesFromClosed();
    }
}
