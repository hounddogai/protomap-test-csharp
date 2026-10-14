namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class LenientAccessBadgeEntryStoreChecks
{
    public static void MemoryLenientAccessBadgeEntryStoreFindsWhatItSaved()
    {
        var store = LenientAccessBadgeEntryStore.NewMemoryLenientAccessBadgeEntryStore();
        var item = LenientAccessBadgeEntry.NewLenientAccessBadgeEntry("code-94", "title-131");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryLenientAccessBadgeEntryStoreFindsWhatItSaved();
    }
}
