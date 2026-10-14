namespace Acme.Workforce.Assets.Tests.Refresh.Estimate;

using Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RefreshCheckpointStoreChecks
{
    public static void MemoryRefreshCheckpointStoreFindsWhatItSaved()
    {
        var store = RefreshCheckpointStore.NewMemoryRefreshCheckpointStore();
        var item = RefreshCheckpoint.NewRefreshCheckpoint("code-711", "title-152");
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
        MemoryRefreshCheckpointStoreFindsWhatItSaved();
    }
}
