namespace Acme.Workforce.Workplace.Tests.Desk.Profile;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DeskRollupChecks
{
    public static void DeskRollupStartsInIdle()
    {
        var item = DeskRollup.NewDeskRollup("code-457", "name-966");
        Check.Equal(item.Status, DeskRollupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskRollupMovesToReady()
    {
        var item = DeskRollup.NewDeskRollup("code-457", "name-966");
        try
        {
            item.MoveTo(DeskRollupStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskRollupStatus.Ready, "status");
    }

    public static void DeskRollupRejectsMovesFromStopped()
    {
        var item = DeskRollup.NewDeskRollup("code-457", "name-966");
        item.Status = DeskRollupStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeskRollupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeskRollupStartsInIdle();
        DeskRollupMovesToReady();
        DeskRollupRejectsMovesFromStopped();
    }
}
