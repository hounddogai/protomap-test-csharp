namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingBundle;

using Acme.Workforce.Contractors.Offboarding.OffboardingBundle;

/// <summary>Checks of offboarding bundle code, run by the project's runner.</summary>
public static class StrictOffboardingBundleChecks
{
    public static void StrictOffboardingBundleStartsInProposed()
    {
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("code-565", "label-572");
        Check.Equal(item.Status, StrictOffboardingBundleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictOffboardingBundleMovesToApproved()
    {
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("code-565", "label-572");
        try
        {
            item.MoveTo(StrictOffboardingBundleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictOffboardingBundleStatus.Approved, "status");
    }

    public static void StrictOffboardingBundleRejectsMovesFromWithdrawn()
    {
        var item = StrictOffboardingBundle.NewStrictOffboardingBundle("code-565", "label-572");
        item.Status = StrictOffboardingBundleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictOffboardingBundleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictOffboardingBundleStartsInProposed();
        StrictOffboardingBundleMovesToApproved();
        StrictOffboardingBundleRejectsMovesFromWithdrawn();
    }
}
