namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class GlobalAttestationBundleChecks
{
    public static void GlobalAttestationBundleStartsInCreated()
    {
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("code-368", "name-946");
        Check.Equal(item.Status, GlobalAttestationBundleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAttestationBundleMovesToValidated()
    {
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("code-368", "name-946");
        try
        {
            item.MoveTo(GlobalAttestationBundleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAttestationBundleStatus.Validated, "status");
    }

    public static void GlobalAttestationBundleRejectsMovesFromRetired()
    {
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("code-368", "name-946");
        item.Status = GlobalAttestationBundleStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalAttestationBundleStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalAttestationBundleStartsInCreated();
        GlobalAttestationBundleMovesToValidated();
        GlobalAttestationBundleRejectsMovesFromRetired();
    }
}
