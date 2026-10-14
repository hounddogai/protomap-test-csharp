namespace Acme.Workforce.Contractors.Tests.Timesheet.Request;

using Acme.Workforce.Contractors.Timesheet.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class TimesheetCycleStoreChecks
{
    public static void MemoryTimesheetCycleStoreFindsWhatItSaved()
    {
        var store = TimesheetCycleStore.NewMemoryTimesheetCycleStore();
        var item = TimesheetCycle.NewTimesheetCycle("code-668", "label-712");
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
        MemoryTimesheetCycleStoreFindsWhatItSaved();
    }
}
