namespace Acme.Workforce.Contractors.Tests.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class TimesheetRecordStoreChecks
{
    public static void MemoryTimesheetRecordStoreFindsWhatItSaved()
    {
        var store = TimesheetRecordStore.NewMemoryTimesheetRecordStore();
        var item = TimesheetRecord.NewTimesheetRecord("code-198", "name-432");
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
        MemoryTimesheetRecordStoreFindsWhatItSaved();
    }
}
