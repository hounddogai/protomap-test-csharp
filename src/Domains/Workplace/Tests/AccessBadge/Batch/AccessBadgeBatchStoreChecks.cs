namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeBatchStoreChecks
{
    public static void MemoryAccessBadgeBatchStoreFindsWhatItSaved()
    {
        var store = AccessBadgeBatchStore.NewMemoryAccessBadgeBatchStore();
        var item = AccessBadgeBatch.NewAccessBadgeBatch("code-739", "name-687");
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
        MemoryAccessBadgeBatchStoreFindsWhatItSaved();
    }
}
