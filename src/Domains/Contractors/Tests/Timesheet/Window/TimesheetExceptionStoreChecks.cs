namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TimesheetExceptionStoreChecks
{
    public static void MemoryTimesheetExceptionStoreFindsWhatItSaved()
    {
        var store = TimesheetExceptionStore.NewMemoryTimesheetExceptionStore();
        var item = TimesheetException.NewTimesheetException("code-160", "title-260");
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
        MemoryTimesheetExceptionStoreFindsWhatItSaved();
    }
}
