namespace Acme.Workforce.Compensation.Tests.MeritCycle.Batch;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingMeritCycleBatchStoreChecks
{
    public static void MemoryPendingMeritCycleBatchStoreFindsWhatItSaved()
    {
        var store = PendingMeritCycleBatchStore.NewMemoryPendingMeritCycleBatchStore();
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("code-524", "title-208");
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
        MemoryPendingMeritCycleBatchStoreFindsWhatItSaved();
    }
}
