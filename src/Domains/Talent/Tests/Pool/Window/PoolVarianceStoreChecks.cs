namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolVarianceStoreChecks
{
    public static void MemoryPoolVarianceStoreFindsWhatItSaved()
    {
        var store = PoolVarianceStore.NewMemoryPoolVarianceStore();
        var item = PoolVariance.NewPoolVariance("code-534", "label-899");
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
        MemoryPoolVarianceStoreFindsWhatItSaved();
    }
}
