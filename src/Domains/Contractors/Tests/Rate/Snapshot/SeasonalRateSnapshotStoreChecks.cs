namespace Acme.Workforce.Contractors.Tests.Rate.Snapshot;

using Acme.Workforce.Contractors.Rate.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalRateSnapshotStoreChecks
{
    public static void MemorySeasonalRateSnapshotStoreFindsWhatItSaved()
    {
        var store = SeasonalRateSnapshotStore.NewMemorySeasonalRateSnapshotStore();
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("code-932", "label-16");
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
        MemorySeasonalRateSnapshotStoreFindsWhatItSaved();
    }
}
