namespace Acme.Workforce.Assets.Tests.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Refresh.RefreshRequest;

/// <summary>Checks of refresh request code, run by the project's runner.</summary>
public static class GlobalRefreshOverrideChecks
{
    public static void GlobalRefreshOverrideStartsInIdle()
    {
        var item = GlobalRefreshOverride.NewGlobalRefreshOverride("code-486", "name-402");
        Check.Equal(item.Status, GlobalRefreshOverrideStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalRefreshOverrideMovesToDraining()
    {
        var item = GlobalRefreshOverride.NewGlobalRefreshOverride("code-486", "name-402");
        try
        {
            item.MoveTo(GlobalRefreshOverrideStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalRefreshOverrideStatus.Draining, "status");
    }

    public static void GlobalRefreshOverrideRejectsMovesFromStopped()
    {
        var item = GlobalRefreshOverride.NewGlobalRefreshOverride("code-486", "name-402");
        item.Status = GlobalRefreshOverrideStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalRefreshOverrideStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalRefreshOverrideStartsInIdle();
        GlobalRefreshOverrideMovesToDraining();
        GlobalRefreshOverrideRejectsMovesFromStopped();
    }
}
