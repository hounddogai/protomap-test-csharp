namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingRollupChecks
{
    public static void OffboardingRollupStartsInIdle()
    {
        var item = OffboardingRollup.NewOffboardingRollup("code-301", "label-554");
        Check.Equal(item.Status, OffboardingRollupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingRollupMovesToDraining()
    {
        var item = OffboardingRollup.NewOffboardingRollup("code-301", "label-554");
        try
        {
            item.MoveTo(OffboardingRollupStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingRollupStatus.Draining, "status");
    }

    public static void OffboardingRollupRejectsMovesFromStopped()
    {
        var item = OffboardingRollup.NewOffboardingRollup("code-301", "label-554");
        item.Status = OffboardingRollupStatus.Stopped;
        Check.Fails(() => item.MoveTo(OffboardingRollupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        OffboardingRollupStartsInIdle();
        OffboardingRollupMovesToDraining();
        OffboardingRollupRejectsMovesFromStopped();
    }
}
