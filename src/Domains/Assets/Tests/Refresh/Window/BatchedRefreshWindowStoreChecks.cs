namespace Acme.Workforce.Assets.Tests.Refresh.Window;

using Acme.Workforce.Assets.Refresh.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BatchedRefreshWindowStoreChecks
{
    public static void MemoryBatchedRefreshWindowStoreFindsWhatItSaved()
    {
        var store = BatchedRefreshWindowStore.NewMemoryBatchedRefreshWindowStore();
        var item = BatchedRefreshWindow.NewBatchedRefreshWindow("code-120", "label-347");
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
        MemoryBatchedRefreshWindowStoreFindsWhatItSaved();
    }
}
