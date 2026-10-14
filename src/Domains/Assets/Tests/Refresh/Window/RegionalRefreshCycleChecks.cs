namespace Acme.Workforce.Assets.Tests.Refresh.Window;

using Acme.Workforce.Assets.Refresh.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalRefreshCycleChecks
{
    public static void RegionalRefreshCycleStartsInIdle()
    {
        var item = RegionalRefreshCycle.NewRegionalRefreshCycle("code-199", "name-723");
        Check.Equal(item.Status, RegionalRefreshCycleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalRefreshCycleMovesToReady()
    {
        var item = RegionalRefreshCycle.NewRegionalRefreshCycle("code-199", "name-723");
        try
        {
            item.MoveTo(RegionalRefreshCycleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalRefreshCycleStatus.Ready, "status");
    }

    public static void RegionalRefreshCycleRejectsMovesFromStopped()
    {
        var item = RegionalRefreshCycle.NewRegionalRefreshCycle("code-199", "name-723");
        item.Status = RegionalRefreshCycleStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalRefreshCycleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalRefreshCycleStartsInIdle();
        RegionalRefreshCycleMovesToReady();
        RegionalRefreshCycleRejectsMovesFromStopped();
    }
}
