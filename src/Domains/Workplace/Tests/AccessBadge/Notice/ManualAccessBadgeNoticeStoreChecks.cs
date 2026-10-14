namespace Acme.Workforce.Workplace.Tests.AccessBadge.Notice;

using Acme.Workforce.Workplace.AccessBadge.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ManualAccessBadgeNoticeStoreChecks
{
    public static void MemoryManualAccessBadgeNoticeStoreFindsWhatItSaved()
    {
        var store = ManualAccessBadgeNoticeStore.NewMemoryManualAccessBadgeNoticeStore();
        var item = ManualAccessBadgeNotice.NewManualAccessBadgeNotice("code-998", "name-643");
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
        MemoryManualAccessBadgeNoticeStoreFindsWhatItSaved();
    }
}
