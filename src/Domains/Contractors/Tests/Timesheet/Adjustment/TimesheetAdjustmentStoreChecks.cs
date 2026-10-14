namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetAdjustmentStoreChecks
{
    public static void MemoryTimesheetAdjustmentStoreFindsWhatItSaved()
    {
        var store = TimesheetAdjustmentStore.NewMemoryTimesheetAdjustmentStore();
        var item = TimesheetAdjustment.NewTimesheetAdjustment("code-820", "title-220");
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
        MemoryTimesheetAdjustmentStoreFindsWhatItSaved();
    }
}
