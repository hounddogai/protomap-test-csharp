namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidaySnapshotStoreChecks
{
    public static void MemoryHolidaySnapshotStoreFindsWhatItSaved()
    {
        var store = HolidaySnapshotStore.NewMemoryHolidaySnapshotStore();
        var item = HolidaySnapshot.NewHolidaySnapshot("code-131", "name-307");
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
        MemoryHolidaySnapshotStoreFindsWhatItSaved();
    }
}
