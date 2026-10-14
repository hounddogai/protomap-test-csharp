namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetSegmentStoreChecks
{
    public static void MemoryTimesheetSegmentStoreFindsWhatItSaved()
    {
        var store = TimesheetSegmentStore.NewMemoryTimesheetSegmentStore();
        var item = TimesheetSegment.NewTimesheetSegment("code-494", "name-332");
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
        MemoryTimesheetSegmentStoreFindsWhatItSaved();
    }
}
