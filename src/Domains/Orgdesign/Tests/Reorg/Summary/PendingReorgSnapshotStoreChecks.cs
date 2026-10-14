namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingReorgSnapshotStoreChecks
{
    public static void MemoryPendingReorgSnapshotStoreFindsWhatItSaved()
    {
        var store = PendingReorgSnapshotStore.NewMemoryPendingReorgSnapshotStore();
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("code-788", "label-433");
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
        MemoryPendingReorgSnapshotStoreFindsWhatItSaved();
    }
}
