namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingReorgEntryChecks
{
    public static void PendingReorgEntryStartsInDraft()
    {
        var item = PendingReorgEntry.NewPendingReorgEntry("code-206", "title-841");
        Check.Equal(item.Status, PendingReorgEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingReorgEntryMovesToActive()
    {
        var item = PendingReorgEntry.NewPendingReorgEntry("code-206", "title-841");
        try
        {
            item.MoveTo(PendingReorgEntryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingReorgEntryStatus.Active, "status");
    }

    public static void PendingReorgEntryRejectsMovesFromClosed()
    {
        var item = PendingReorgEntry.NewPendingReorgEntry("code-206", "title-841");
        item.Status = PendingReorgEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(PendingReorgEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PendingReorgEntryStartsInDraft();
        PendingReorgEntryMovesToActive();
        PendingReorgEntryRejectsMovesFromClosed();
    }
}
