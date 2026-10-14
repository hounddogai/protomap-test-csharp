namespace Acme.Workforce.Contractors.Tests.Timesheet.Batch;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedTimesheetGroupStoreChecks
{
    public static void MemoryCappedTimesheetGroupStoreFindsWhatItSaved()
    {
        var store = CappedTimesheetGroupStore.NewMemoryCappedTimesheetGroupStore();
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("code-900", "name-137");
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
        MemoryCappedTimesheetGroupStoreFindsWhatItSaved();
    }
}
