namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolAuditStoreChecks
{
    public static void MemoryPoolAuditStoreFindsWhatItSaved()
    {
        var store = PoolAuditStore.NewMemoryPoolAuditStore();
        var item = PoolAudit.NewPoolAudit("code-510", "title-142");
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
        MemoryPoolAuditStoreFindsWhatItSaved();
    }
}
