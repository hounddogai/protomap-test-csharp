namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshScoreChecks
{
    public static void RefreshScoreStartsInPlanned()
    {
        var item = RefreshScore.NewRefreshScore("code-57", "name-499");
        Check.Equal(item.Status, RefreshScoreStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshScoreMovesToInProgress()
    {
        var item = RefreshScore.NewRefreshScore("code-57", "name-499");
        try
        {
            item.MoveTo(RefreshScoreStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshScoreStatus.InProgress, "status");
    }

    public static void RefreshScoreRejectsMovesFromCancelled()
    {
        var item = RefreshScore.NewRefreshScore("code-57", "name-499");
        item.Status = RefreshScoreStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RefreshScoreStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RefreshScoreStartsInPlanned();
        RefreshScoreMovesToInProgress();
        RefreshScoreRejectsMovesFromCancelled();
    }
}
