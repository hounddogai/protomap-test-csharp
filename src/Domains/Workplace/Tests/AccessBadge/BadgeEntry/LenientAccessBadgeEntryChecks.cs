namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class LenientAccessBadgeEntryChecks
{
    public static void LenientAccessBadgeEntryStartsInNew()
    {
        var item = LenientAccessBadgeEntry.NewLenientAccessBadgeEntry("code-94", "title-131");
        Check.Equal(item.Status, LenientAccessBadgeEntryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientAccessBadgeEntryMovesToQueued()
    {
        var item = LenientAccessBadgeEntry.NewLenientAccessBadgeEntry("code-94", "title-131");
        try
        {
            item.MoveTo(LenientAccessBadgeEntryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientAccessBadgeEntryStatus.Queued, "status");
    }

    public static void LenientAccessBadgeEntryRejectsMovesFromFailed()
    {
        var item = LenientAccessBadgeEntry.NewLenientAccessBadgeEntry("code-94", "title-131");
        item.Status = LenientAccessBadgeEntryStatus.Failed;
        Check.Fails(() => item.MoveTo(LenientAccessBadgeEntryStatus.New), "move");
    }

    public static void RunAll()
    {
        LenientAccessBadgeEntryStartsInNew();
        LenientAccessBadgeEntryMovesToQueued();
        LenientAccessBadgeEntryRejectsMovesFromFailed();
    }
}
