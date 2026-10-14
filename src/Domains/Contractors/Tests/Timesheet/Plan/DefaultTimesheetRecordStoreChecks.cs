namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultTimesheetRecordStoreChecks
{
    public static void MemoryDefaultTimesheetRecordStoreFindsWhatItSaved()
    {
        var store = DefaultTimesheetRecordStore.NewMemoryDefaultTimesheetRecordStore();
        var item = DefaultTimesheetRecord.NewDefaultTimesheetRecord("code-250", "name-74");
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
        MemoryDefaultTimesheetRecordStoreFindsWhatItSaved();
    }
}
