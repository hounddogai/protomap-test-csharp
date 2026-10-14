namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class BatchedPoolAuditStoreChecks
{
    public static void MemoryBatchedPoolAuditStoreFindsWhatItSaved()
    {
        var store = BatchedPoolAuditStore.NewMemoryBatchedPoolAuditStore();
        var item = BatchedPoolAudit.NewBatchedPoolAudit("code-782", "name-650");
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
        MemoryBatchedPoolAuditStoreFindsWhatItSaved();
    }
}
