namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class BatchedHolidayScheduleStoreChecks
{
    public static void MemoryBatchedHolidayScheduleStoreFindsWhatItSaved()
    {
        var store = BatchedHolidayScheduleStore.NewMemoryBatchedHolidayScheduleStore();
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("code-1", "name-74");
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
        MemoryBatchedHolidayScheduleStoreFindsWhatItSaved();
    }
}
