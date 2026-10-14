namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackDeskSnapshotStoreChecks
{
    public static void MemoryFallbackDeskSnapshotStoreFindsWhatItSaved()
    {
        var store = FallbackDeskSnapshotStore.NewMemoryFallbackDeskSnapshotStore();
        var item = FallbackDeskSnapshot.NewFallbackDeskSnapshot("code-285", "title-457");
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
        MemoryFallbackDeskSnapshotStoreFindsWhatItSaved();
    }
}
