namespace Acme.Workforce.Compliance.Tests.Attestation.Adjustment;

using Acme.Workforce.Compliance.Attestation.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PrimaryAttestationPolicyChecks
{
    public static void PrimaryAttestationPolicyStartsInPlanned()
    {
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("code-959", "name-898");
        Check.Equal(item.Status, PrimaryAttestationPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryAttestationPolicyMovesToInProgress()
    {
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("code-959", "name-898");
        try
        {
            item.MoveTo(PrimaryAttestationPolicyStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryAttestationPolicyStatus.InProgress, "status");
    }

    public static void PrimaryAttestationPolicyRejectsMovesFromCancelled()
    {
        var item = PrimaryAttestationPolicy.NewPrimaryAttestationPolicy("code-959", "name-898");
        item.Status = PrimaryAttestationPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PrimaryAttestationPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PrimaryAttestationPolicyStartsInPlanned();
        PrimaryAttestationPolicyMovesToInProgress();
        PrimaryAttestationPolicyRejectsMovesFromCancelled();
    }
}
