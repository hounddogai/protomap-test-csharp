namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WeightedRefreshScoreChecks
{
    public static void WeightedRefreshScoreStartsInPlanned()
    {
        var item = WeightedRefreshScore.NewWeightedRefreshScore("code-195", "label-272");
        Check.Equal(item.Status, WeightedRefreshScoreStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRefreshScoreMovesToScheduled()
    {
        var item = WeightedRefreshScore.NewWeightedRefreshScore("code-195", "label-272");
        try
        {
            item.MoveTo(WeightedRefreshScoreStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRefreshScoreStatus.Scheduled, "status");
    }

    public static void WeightedRefreshScoreRejectsMovesFromCancelled()
    {
        var item = WeightedRefreshScore.NewWeightedRefreshScore("code-195", "label-272");
        item.Status = WeightedRefreshScoreStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedRefreshScoreStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedRefreshScoreStartsInPlanned();
        WeightedRefreshScoreMovesToScheduled();
        WeightedRefreshScoreRejectsMovesFromCancelled();
    }
}
