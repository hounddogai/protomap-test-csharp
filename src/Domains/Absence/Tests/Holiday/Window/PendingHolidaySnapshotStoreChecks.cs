namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PendingHolidaySnapshotStoreChecks
{
    public static void MemoryPendingHolidaySnapshotStoreFindsWhatItSaved()
    {
        var store = PendingHolidaySnapshotStore.NewMemoryPendingHolidaySnapshotStore();
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("code-378", "name-699");
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
        MemoryPendingHolidaySnapshotStoreFindsWhatItSaved();
    }
}
