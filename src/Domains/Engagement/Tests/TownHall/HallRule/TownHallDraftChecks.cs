namespace Acme.Workforce.Engagement.Tests.TownHall.HallRule;

using Acme.Workforce.Engagement.TownHall.HallRule;

/// <summary>Checks of hall rule code, run by the project's runner.</summary>
public static class TownHallDraftChecks
{
    public static void TownHallDraftStartsInPlanned()
    {
        var item = TownHallDraft.NewTownHallDraft("code-59", "name-464");
        Check.Equal(item.Status, TownHallDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallDraftMovesToScheduled()
    {
        var item = TownHallDraft.NewTownHallDraft("code-59", "name-464");
        try
        {
            item.MoveTo(TownHallDraftStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallDraftStatus.Scheduled, "status");
    }

    public static void TownHallDraftRejectsMovesFromCancelled()
    {
        var item = TownHallDraft.NewTownHallDraft("code-59", "name-464");
        item.Status = TownHallDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TownHallDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TownHallDraftStartsInPlanned();
        TownHallDraftMovesToScheduled();
        TownHallDraftRejectsMovesFromCancelled();
    }
}
