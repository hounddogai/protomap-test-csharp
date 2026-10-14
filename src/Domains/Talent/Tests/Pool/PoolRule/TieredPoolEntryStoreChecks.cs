namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class TieredPoolEntryStoreChecks
{
    public static void MemoryTieredPoolEntryStoreFindsWhatItSaved()
    {
        var store = TieredPoolEntryStore.NewMemoryTieredPoolEntryStore();
        var item = TieredPoolEntry.NewTieredPoolEntry("code-746", "name-262");
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
        MemoryTieredPoolEntryStoreFindsWhatItSaved();
    }
}
