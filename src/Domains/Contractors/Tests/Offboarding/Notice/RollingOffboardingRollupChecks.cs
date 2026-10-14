namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingOffboardingRollupChecks
{
    public static void RollingOffboardingRollupStartsInDraft()
    {
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("code-880", "label-675");
        Check.Equal(item.Status, RollingOffboardingRollupStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingOffboardingRollupMovesToPending()
    {
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("code-880", "label-675");
        try
        {
            item.MoveTo(RollingOffboardingRollupStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingOffboardingRollupStatus.Pending, "status");
    }

    public static void RollingOffboardingRollupRejectsMovesFromClosed()
    {
        var item = RollingOffboardingRollup.NewRollingOffboardingRollup("code-880", "label-675");
        item.Status = RollingOffboardingRollupStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingOffboardingRollupStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingOffboardingRollupStartsInDraft();
        RollingOffboardingRollupMovesToPending();
        RollingOffboardingRollupRejectsMovesFromClosed();
    }
}
