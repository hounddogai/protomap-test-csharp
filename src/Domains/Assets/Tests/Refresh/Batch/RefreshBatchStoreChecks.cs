namespace Acme.Workforce.Assets.Tests.Refresh.Batch;

using Acme.Workforce.Assets.Refresh.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RefreshBatchStoreChecks
{
    public static void MemoryRefreshBatchStoreFindsWhatItSaved()
    {
        var store = RefreshBatchStore.NewMemoryRefreshBatchStore();
        var item = RefreshBatch.NewRefreshBatch("code-366", "label-821");
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
        MemoryRefreshBatchStoreFindsWhatItSaved();
    }
}
