namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TimesheetAllocationStoreChecks
{
    public static void MemoryTimesheetAllocationStoreFindsWhatItSaved()
    {
        var store = TimesheetAllocationStore.NewMemoryTimesheetAllocationStore();
        var item = TimesheetAllocation.NewTimesheetAllocation("code-437", "label-358");
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
        MemoryTimesheetAllocationStoreFindsWhatItSaved();
    }
}
