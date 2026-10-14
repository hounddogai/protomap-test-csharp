namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingTierChecks
{
    public static void OffboardingTierStartsInIdle()
    {
        var item = OffboardingTier.NewOffboardingTier("code-527", "label-827");
        Check.Equal(item.Status, OffboardingTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingTierMovesToWarming()
    {
        var item = OffboardingTier.NewOffboardingTier("code-527", "label-827");
        try
        {
            item.MoveTo(OffboardingTierStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingTierStatus.Warming, "status");
    }

    public static void OffboardingTierRejectsMovesFromStopped()
    {
        var item = OffboardingTier.NewOffboardingTier("code-527", "label-827");
        item.Status = OffboardingTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(OffboardingTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        OffboardingTierStartsInIdle();
        OffboardingTierMovesToWarming();
        OffboardingTierRejectsMovesFromStopped();
    }
}
