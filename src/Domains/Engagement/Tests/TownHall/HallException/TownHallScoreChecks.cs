namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallScoreChecks
{
    public static void TownHallScoreStartsInDraft()
    {
        var item = TownHallScore.NewTownHallScore("code-231", "name-473");
        Check.Equal(item.Status, TownHallScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallScoreMovesToActive()
    {
        var item = TownHallScore.NewTownHallScore("code-231", "name-473");
        try
        {
            item.MoveTo(TownHallScoreStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallScoreStatus.Active, "status");
    }

    public static void TownHallScoreRejectsMovesFromClosed()
    {
        var item = TownHallScore.NewTownHallScore("code-231", "name-473");
        item.Status = TownHallScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(TownHallScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TownHallScoreStartsInDraft();
        TownHallScoreMovesToActive();
        TownHallScoreRejectsMovesFromClosed();
    }
}
