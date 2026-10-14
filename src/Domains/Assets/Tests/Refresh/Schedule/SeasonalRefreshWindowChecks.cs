namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class SeasonalRefreshWindowChecks
{
    public static void SeasonalRefreshWindowStartsInProposed()
    {
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("code-555", "title-218");
        Check.Equal(item.Status, SeasonalRefreshWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalRefreshWindowMovesToRejected()
    {
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("code-555", "title-218");
        try
        {
            item.MoveTo(SeasonalRefreshWindowStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalRefreshWindowStatus.Rejected, "status");
    }

    public static void SeasonalRefreshWindowRejectsMovesFromWithdrawn()
    {
        var item = SeasonalRefreshWindow.NewSeasonalRefreshWindow("code-555", "title-218");
        item.Status = SeasonalRefreshWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalRefreshWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalRefreshWindowStartsInProposed();
        SeasonalRefreshWindowMovesToRejected();
        SeasonalRefreshWindowRejectsMovesFromWithdrawn();
    }
}
