namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TownHallBundleChecks
{
    public static void TownHallBundleStartsInNew()
    {
        var item = TownHallBundle.NewTownHallBundle("code-452", "name-546");
        Check.Equal(item.Status, TownHallBundleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallBundleMovesToRunning()
    {
        var item = TownHallBundle.NewTownHallBundle("code-452", "name-546");
        try
        {
            item.MoveTo(TownHallBundleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallBundleStatus.Running, "status");
    }

    public static void TownHallBundleRejectsMovesFromFailed()
    {
        var item = TownHallBundle.NewTownHallBundle("code-452", "name-546");
        item.Status = TownHallBundleStatus.Failed;
        Check.Fails(() => item.MoveTo(TownHallBundleStatus.New), "move");
    }

    public static void RunAll()
    {
        TownHallBundleStartsInNew();
        TownHallBundleMovesToRunning();
        TownHallBundleRejectsMovesFromFailed();
    }
}
