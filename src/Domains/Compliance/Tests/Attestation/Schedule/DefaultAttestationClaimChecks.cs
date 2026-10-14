namespace Acme.Workforce.Compliance.Tests.Attestation.Schedule;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DefaultAttestationClaimChecks
{
    public static void DefaultAttestationClaimStartsInProposed()
    {
        var item = DefaultAttestationClaim.NewDefaultAttestationClaim("code-884", "name-957");
        Check.Equal(item.Status, DefaultAttestationClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultAttestationClaimMovesToWithdrawn()
    {
        var item = DefaultAttestationClaim.NewDefaultAttestationClaim("code-884", "name-957");
        try
        {
            item.MoveTo(DefaultAttestationClaimStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultAttestationClaimStatus.Withdrawn, "status");
    }

    public static void DefaultAttestationClaimRejectsMovesFromWithdrawn()
    {
        var item = DefaultAttestationClaim.NewDefaultAttestationClaim("code-884", "name-957");
        item.Status = DefaultAttestationClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultAttestationClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultAttestationClaimStartsInProposed();
        DefaultAttestationClaimMovesToWithdrawn();
        DefaultAttestationClaimRejectsMovesFromWithdrawn();
    }
}
