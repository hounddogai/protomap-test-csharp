namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TownHallEntryChecks
{
    public static void TownHallEntryStartsInNew()
    {
        var item = TownHallEntry.NewTownHallEntry("code-982", "name-465");
        Check.Equal(item.Status, TownHallEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallEntryMovesToQueued()
    {
        var item = TownHallEntry.NewTownHallEntry("code-982", "name-465");
        try
        {
            item.MoveTo(TownHallEntryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallEntryStatus.Queued, "status");
    }

    public static void TownHallEntryRejectsMovesFromFailed()
    {
        var item = TownHallEntry.NewTownHallEntry("code-982", "name-465");
        item.Status = TownHallEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(TownHallEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        TownHallEntryStartsInNew();
        TownHallEntryMovesToQueued();
        TownHallEntryRejectsMovesFromFailed();
    }
}
