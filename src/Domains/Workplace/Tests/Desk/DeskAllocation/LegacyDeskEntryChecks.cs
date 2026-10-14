namespace Acme.Workforce.Workplace.Tests.Desk.DeskAllocation;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Checks of desk allocation code, run by the project's runner.</summary>
public static class LegacyDeskEntryChecks
{
    public static void LegacyDeskEntryStartsInDraft()
    {
        var item = LegacyDeskEntry.NewLegacyDeskEntry("code-228", "title-166");
        Check.Equal(item.Status, LegacyDeskEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyDeskEntryMovesToActive()
    {
        var item = LegacyDeskEntry.NewLegacyDeskEntry("code-228", "title-166");
        try
        {
            item.MoveTo(LegacyDeskEntryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyDeskEntryStatus.Active, "status");
    }

    public static void LegacyDeskEntryRejectsMovesFromClosed()
    {
        var item = LegacyDeskEntry.NewLegacyDeskEntry("code-228", "title-166");
        item.Status = LegacyDeskEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyDeskEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyDeskEntryStartsInDraft();
        LegacyDeskEntryMovesToActive();
        LegacyDeskEntryRejectsMovesFromClosed();
    }
}
