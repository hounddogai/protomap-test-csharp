namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalReorgSnapshotStoreChecks
{
    public static void MemorySeasonalReorgSnapshotStoreFindsWhatItSaved()
    {
        var store = SeasonalReorgSnapshotStore.NewMemorySeasonalReorgSnapshotStore();
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("code-478", "label-39");
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
        MemorySeasonalReorgSnapshotStoreFindsWhatItSaved();
    }
}
