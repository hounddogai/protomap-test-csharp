namespace Acme.Workforce.Contractors.Tests.Timesheet.Profile;

using Acme.Workforce.Contractors.Timesheet.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class TimesheetBatchStoreChecks
{
    public static void MemoryTimesheetBatchStoreFindsWhatItSaved()
    {
        var store = TimesheetBatchStore.NewMemoryTimesheetBatchStore();
        var item = TimesheetBatch.NewTimesheetBatch("code-368", "name-850");
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
        MemoryTimesheetBatchStoreFindsWhatItSaved();
    }
}
