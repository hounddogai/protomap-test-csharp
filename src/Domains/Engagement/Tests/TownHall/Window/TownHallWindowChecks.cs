namespace Acme.Workforce.Engagement.Tests.TownHall.Window;

using Acme.Workforce.Engagement.TownHall.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TownHallWindowChecks
{
    public static void TownHallWindowStartsInOpen()
    {
        var item = TownHallWindow.NewTownHallWindow("code-54", "title-666");
        Check.Equal(item.Status, TownHallWindowStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallWindowMovesToArchived()
    {
        var item = TownHallWindow.NewTownHallWindow("code-54", "title-666");
        try
        {
            item.MoveTo(TownHallWindowStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallWindowStatus.Archived, "status");
    }

    public static void TownHallWindowRejectsMovesFromArchived()
    {
        var item = TownHallWindow.NewTownHallWindow("code-54", "title-666");
        item.Status = TownHallWindowStatus.Archived;
        Check.Fails(() => item.MoveTo(TownHallWindowStatus.Open), "move");
    }

    public static void RunAll()
    {
        TownHallWindowStartsInOpen();
        TownHallWindowMovesToArchived();
        TownHallWindowRejectsMovesFromArchived();
    }
}
