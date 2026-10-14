namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class FallbackHolidaySnapshotStoreChecks
{
    public static void MemoryFallbackHolidaySnapshotStoreFindsWhatItSaved()
    {
        var store = FallbackHolidaySnapshotStore.NewMemoryFallbackHolidaySnapshotStore();
        var item = FallbackHolidaySnapshot.NewFallbackHolidaySnapshot("code-194", "label-2");
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
        MemoryFallbackHolidaySnapshotStoreFindsWhatItSaved();
    }
}
