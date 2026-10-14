namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PoolSnapshotStoreChecks
{
    public static void MemoryPoolSnapshotStoreFindsWhatItSaved()
    {
        var store = PoolSnapshotStore.NewMemoryPoolSnapshotStore();
        var item = PoolSnapshot.NewPoolSnapshot("code-780", "title-241");
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
        MemoryPoolSnapshotStoreFindsWhatItSaved();
    }
}
