namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedAccessBadgeDraftStoreChecks
{
    public static void MemoryCappedAccessBadgeDraftStoreFindsWhatItSaved()
    {
        var store = CappedAccessBadgeDraftStore.NewMemoryCappedAccessBadgeDraftStore();
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("code-412", "label-655");
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
        MemoryCappedAccessBadgeDraftStoreFindsWhatItSaved();
    }
}
