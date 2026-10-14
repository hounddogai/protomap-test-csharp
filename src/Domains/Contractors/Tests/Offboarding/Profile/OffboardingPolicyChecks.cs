namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingPolicyChecks
{
    public static void OffboardingPolicyStartsInIdle()
    {
        var item = OffboardingPolicy.NewOffboardingPolicy("code-331", "title-199");
        Check.Equal(item.Status, OffboardingPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingPolicyMovesToStopped()
    {
        var item = OffboardingPolicy.NewOffboardingPolicy("code-331", "title-199");
        try
        {
            item.MoveTo(OffboardingPolicyStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingPolicyStatus.Stopped, "status");
    }

    public static void OffboardingPolicyRejectsMovesFromStopped()
    {
        var item = OffboardingPolicy.NewOffboardingPolicy("code-331", "title-199");
        item.Status = OffboardingPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(OffboardingPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        OffboardingPolicyStartsInIdle();
        OffboardingPolicyMovesToStopped();
        OffboardingPolicyRejectsMovesFromStopped();
    }
}
