namespace Acme.Workforce.Compliance.Tests.Attestation.Baseline;

using Acme.Workforce.Compliance.Attestation.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalAttestationBaselineChecks
{
    public static void GlobalAttestationBaselineStartsInProposed()
    {
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("code-731", "label-485");
        Check.Equal(item.Status, GlobalAttestationBaselineStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAttestationBaselineMovesToApproved()
    {
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("code-731", "label-485");
        try
        {
            item.MoveTo(GlobalAttestationBaselineStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAttestationBaselineStatus.Approved, "status");
    }

    public static void GlobalAttestationBaselineRejectsMovesFromWithdrawn()
    {
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("code-731", "label-485");
        item.Status = GlobalAttestationBaselineStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GlobalAttestationBaselineStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GlobalAttestationBaselineStartsInProposed();
        GlobalAttestationBaselineMovesToApproved();
        GlobalAttestationBaselineRejectsMovesFromWithdrawn();
    }
}
