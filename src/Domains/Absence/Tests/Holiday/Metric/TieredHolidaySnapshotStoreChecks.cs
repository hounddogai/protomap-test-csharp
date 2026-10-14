namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredHolidaySnapshotStoreChecks
{
    public static void MemoryTieredHolidaySnapshotStoreFindsWhatItSaved()
    {
        var store = TieredHolidaySnapshotStore.NewMemoryTieredHolidaySnapshotStore();
        var item = TieredHolidaySnapshot.NewTieredHolidaySnapshot("code-378", "name-497");
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
        MemoryTieredHolidaySnapshotStoreFindsWhatItSaved();
    }
}
