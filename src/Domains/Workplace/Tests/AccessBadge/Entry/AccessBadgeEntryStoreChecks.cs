namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeEntryStoreChecks
{
    public static void MemoryAccessBadgeEntryStoreFindsWhatItSaved()
    {
        var store = AccessBadgeEntryStore.NewMemoryAccessBadgeEntryStore();
        var item = AccessBadgeEntry.NewAccessBadgeEntry("code-532", "label-819");
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
        MemoryAccessBadgeEntryStoreFindsWhatItSaved();
    }
}
