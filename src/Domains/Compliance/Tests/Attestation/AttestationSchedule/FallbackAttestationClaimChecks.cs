namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class FallbackAttestationClaimChecks
{
    public static void FallbackAttestationClaimStartsInIdle()
    {
        var item = FallbackAttestationClaim.NewFallbackAttestationClaim("code-428", "label-351");
        Check.Equal(item.Status, FallbackAttestationClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAttestationClaimMovesToStopped()
    {
        var item = FallbackAttestationClaim.NewFallbackAttestationClaim("code-428", "label-351");
        try
        {
            item.MoveTo(FallbackAttestationClaimStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAttestationClaimStatus.Stopped, "status");
    }

    public static void FallbackAttestationClaimRejectsMovesFromStopped()
    {
        var item = FallbackAttestationClaim.NewFallbackAttestationClaim("code-428", "label-351");
        item.Status = FallbackAttestationClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackAttestationClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackAttestationClaimStartsInIdle();
        FallbackAttestationClaimMovesToStopped();
        FallbackAttestationClaimRejectsMovesFromStopped();
    }
}
