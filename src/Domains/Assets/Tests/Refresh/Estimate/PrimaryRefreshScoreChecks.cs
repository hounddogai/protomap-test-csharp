namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryRefreshScoreChecks
{
    public static void PrimaryRefreshScoreStartsInIdle()
    {
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("code-801", "title-654");
        Check.Equal(item.Status, PrimaryRefreshScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRefreshScoreMovesToReady()
    {
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("code-801", "title-654");
        try
        {
            item.MoveTo(PrimaryRefreshScoreStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRefreshScoreStatus.Ready, "status");
    }

    public static void PrimaryRefreshScoreRejectsMovesFromStopped()
    {
        var item = PrimaryRefreshScore.NewPrimaryRefreshScore("code-801", "title-654");
        item.Status = PrimaryRefreshScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(PrimaryRefreshScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PrimaryRefreshScoreStartsInIdle();
        PrimaryRefreshScoreMovesToReady();
        PrimaryRefreshScoreRejectsMovesFromStopped();
    }
}
