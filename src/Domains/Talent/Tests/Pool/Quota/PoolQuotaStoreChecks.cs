namespace Acme.Workforce.Talent.Tests.Pool.Quota;

using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PoolQuotaStoreChecks
{
    public static void MemoryPoolQuotaStoreFindsWhatItSaved()
    {
        var store = PoolQuotaStore.NewMemoryPoolQuotaStore();
        var item = PoolQuota.NewPoolQuota("code-125", "label-436");
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
        MemoryPoolQuotaStoreFindsWhatItSaved();
    }
}
