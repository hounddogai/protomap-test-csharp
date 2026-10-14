namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScheduledTimesheetGroupStoreChecks
{
    public static void MemoryScheduledTimesheetGroupStoreFindsWhatItSaved()
    {
        var store = ScheduledTimesheetGroupStore.NewMemoryScheduledTimesheetGroupStore();
        var item = ScheduledTimesheetGroup.NewScheduledTimesheetGroup("code-664", "label-877");
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
        MemoryScheduledTimesheetGroupStoreFindsWhatItSaved();
    }
}
