namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PoolRollupStoreChecks
{
    public static void MemoryPoolRollupStoreFindsWhatItSaved()
    {
        var store = PoolRollupStore.NewMemoryPoolRollupStore();
        var item = PoolRollup.NewPoolRollup("code-887", "title-915");
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
        MemoryPoolRollupStoreFindsWhatItSaved();
    }
}
