namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetGroupStoreChecks
{
    public static void MemoryTimesheetGroupStoreFindsWhatItSaved()
    {
        var store = TimesheetGroupStore.NewMemoryTimesheetGroupStore();
        var item = TimesheetGroup.NewTimesheetGroup("code-131", "title-654");
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
        MemoryTimesheetGroupStoreFindsWhatItSaved();
    }
}
