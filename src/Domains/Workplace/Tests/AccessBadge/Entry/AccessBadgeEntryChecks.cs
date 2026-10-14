namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeEntryChecks
{
    public static void AccessBadgeEntryStartsInDraft()
    {
        var item = AccessBadgeEntry.NewAccessBadgeEntry("code-532", "label-819");
        Check.Equal(item.Status, AccessBadgeEntryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeEntryMovesToActive()
    {
        var item = AccessBadgeEntry.NewAccessBadgeEntry("code-532", "label-819");
        try
        {
            item.MoveTo(AccessBadgeEntryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeEntryStatus.Active, "status");
    }

    public static void AccessBadgeEntryRejectsMovesFromClosed()
    {
        var item = AccessBadgeEntry.NewAccessBadgeEntry("code-532", "label-819");
        item.Status = AccessBadgeEntryStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgeEntryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgeEntryStartsInDraft();
        AccessBadgeEntryMovesToActive();
        AccessBadgeEntryRejectsMovesFromClosed();
    }
}
