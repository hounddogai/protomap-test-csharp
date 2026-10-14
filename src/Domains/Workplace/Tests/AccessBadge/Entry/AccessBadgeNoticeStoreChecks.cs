namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeNoticeStoreChecks
{
    public static void MemoryAccessBadgeNoticeStoreFindsWhatItSaved()
    {
        var store = AccessBadgeNoticeStore.NewMemoryAccessBadgeNoticeStore();
        var item = AccessBadgeNotice.NewAccessBadgeNotice("code-850", "title-260");
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
        MemoryAccessBadgeNoticeStoreFindsWhatItSaved();
    }
}
