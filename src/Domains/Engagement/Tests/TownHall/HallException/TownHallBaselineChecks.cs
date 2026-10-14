namespace Acme.Workforce.Engagement.Tests.TownHall.HallException;

using Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>Checks of hall exception code, run by the project's runner.</summary>
public static class TownHallBaselineChecks
{
    public static void TownHallBaselineStartsInNew()
    {
        var item = TownHallBaseline.NewTownHallBaseline("code-809", "name-29");
        Check.Equal(item.Status, TownHallBaselineStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallBaselineMovesToDone()
    {
        var item = TownHallBaseline.NewTownHallBaseline("code-809", "name-29");
        try
        {
            item.MoveTo(TownHallBaselineStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallBaselineStatus.Done, "status");
    }

    public static void TownHallBaselineRejectsMovesFromFailed()
    {
        var item = TownHallBaseline.NewTownHallBaseline("code-809", "name-29");
        item.Status = TownHallBaselineStatus.Failed;
        Check.Fails(() => item.MoveTo(TownHallBaselineStatus.New), "move");
    }

    public static void RunAll()
    {
        TownHallBaselineStartsInNew();
        TownHallBaselineMovesToDone();
        TownHallBaselineRejectsMovesFromFailed();
    }
}
