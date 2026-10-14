namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyRollupChecks
{
    public static void PolicyRollupStartsInIdle()
    {
        var item = PolicyRollup.NewPolicyRollup("code-777", "title-514");
        Check.Equal(item.Status, PolicyRollupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyRollupMovesToReady()
    {
        var item = PolicyRollup.NewPolicyRollup("code-777", "title-514");
        try
        {
            item.MoveTo(PolicyRollupStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyRollupStatus.Ready, "status");
    }

    public static void PolicyRollupRejectsMovesFromStopped()
    {
        var item = PolicyRollup.NewPolicyRollup("code-777", "title-514");
        item.Status = PolicyRollupStatus.Stopped;
        Check.Fails(() => item.MoveTo(PolicyRollupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PolicyRollupStartsInIdle();
        PolicyRollupMovesToReady();
        PolicyRollupRejectsMovesFromStopped();
    }
}
