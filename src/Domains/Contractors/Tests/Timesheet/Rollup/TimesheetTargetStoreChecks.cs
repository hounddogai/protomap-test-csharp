namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetTargetStoreChecks
{
    public static void MemoryTimesheetTargetStoreFindsWhatItSaved()
    {
        var store = TimesheetTargetStore.NewMemoryTimesheetTargetStore();
        var item = TimesheetTarget.NewTimesheetTarget("code-951", "label-864");
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
        MemoryTimesheetTargetStoreFindsWhatItSaved();
    }
}
