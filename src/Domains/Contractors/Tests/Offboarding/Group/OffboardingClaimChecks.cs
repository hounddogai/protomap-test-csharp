namespace Acme.Workforce.Contractors.Tests.Offboarding.Group;

using Acme.Workforce.Contractors.Offboarding.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class OffboardingClaimChecks
{
    public static void OffboardingClaimStartsInIdle()
    {
        var item = OffboardingClaim.NewOffboardingClaim("code-556", "name-788");
        Check.Equal(item.Status, OffboardingClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingClaimMovesToWarming()
    {
        var item = OffboardingClaim.NewOffboardingClaim("code-556", "name-788");
        try
        {
            item.MoveTo(OffboardingClaimStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingClaimStatus.Warming, "status");
    }

    public static void OffboardingClaimRejectsMovesFromStopped()
    {
        var item = OffboardingClaim.NewOffboardingClaim("code-556", "name-788");
        item.Status = OffboardingClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(OffboardingClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        OffboardingClaimStartsInIdle();
        OffboardingClaimMovesToWarming();
        OffboardingClaimRejectsMovesFromStopped();
    }
}
