namespace Acme.Workforce.Talent.Tests.Pool.Estimate;

using Acme.Workforce.Talent.Pool.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PoolEntryStoreChecks
{
    public static void MemoryPoolEntryStoreFindsWhatItSaved()
    {
        var store = PoolEntryStore.NewMemoryPoolEntryStore();
        var item = PoolEntry.NewPoolEntry("code-808", "name-388");
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
        MemoryPoolEntryStoreFindsWhatItSaved();
    }
}
