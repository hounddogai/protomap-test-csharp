namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReorgSnapshotStoreChecks
{
    public static void MemoryReorgSnapshotStoreFindsWhatItSaved()
    {
        var store = ReorgSnapshotStore.NewMemoryReorgSnapshotStore();
        var item = ReorgSnapshot.NewReorgSnapshot("code-169", "name-298");
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
        MemoryReorgSnapshotStoreFindsWhatItSaved();
    }
}
