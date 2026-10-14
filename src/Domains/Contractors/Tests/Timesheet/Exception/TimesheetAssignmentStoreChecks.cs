namespace Acme.Workforce.Contractors.Tests.Timesheet.Exception;

using Acme.Workforce.Contractors.Timesheet.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TimesheetAssignmentStoreChecks
{
    public static void MemoryTimesheetAssignmentStoreFindsWhatItSaved()
    {
        var store = TimesheetAssignmentStore.NewMemoryTimesheetAssignmentStore();
        var item = TimesheetAssignment.NewTimesheetAssignment("code-408", "title-526");
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
        MemoryTimesheetAssignmentStoreFindsWhatItSaved();
    }
}
