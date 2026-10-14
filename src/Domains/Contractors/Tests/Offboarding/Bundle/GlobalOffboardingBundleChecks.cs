namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class GlobalOffboardingBundleChecks
{
    public static void GlobalOffboardingBundleStartsInProposed()
    {
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("code-933", "name-71");
        Check.Equal(item.Status, GlobalOffboardingBundleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalOffboardingBundleMovesToApproved()
    {
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("code-933", "name-71");
        try
        {
            item.MoveTo(GlobalOffboardingBundleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalOffboardingBundleStatus.Approved, "status");
    }

    public static void GlobalOffboardingBundleRejectsMovesFromWithdrawn()
    {
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("code-933", "name-71");
        item.Status = GlobalOffboardingBundleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GlobalOffboardingBundleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GlobalOffboardingBundleStartsInProposed();
        GlobalOffboardingBundleMovesToApproved();
        GlobalOffboardingBundleRejectsMovesFromWithdrawn();
    }
}
