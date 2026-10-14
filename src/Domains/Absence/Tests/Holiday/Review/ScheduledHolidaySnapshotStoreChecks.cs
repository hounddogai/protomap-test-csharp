namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledHolidaySnapshotStoreChecks
{
    public static void MemoryScheduledHolidaySnapshotStoreFindsWhatItSaved()
    {
        var store = ScheduledHolidaySnapshotStore.NewMemoryScheduledHolidaySnapshotStore();
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("code-563", "label-418");
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
        MemoryScheduledHolidaySnapshotStoreFindsWhatItSaved();
    }
}
