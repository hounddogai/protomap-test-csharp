namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PendingRefreshQuotaStoreChecks
{
    public static void MemoryPendingRefreshQuotaStoreFindsWhatItSaved()
    {
        var store = PendingRefreshQuotaStore.NewMemoryPendingRefreshQuotaStore();
        var item = PendingRefreshQuota.NewPendingRefreshQuota("code-634", "title-884");
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
        MemoryPendingRefreshQuotaStoreFindsWhatItSaved();
    }
}
