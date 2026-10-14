namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetSnapshotStoreChecks
{
    public static void MemoryTimesheetSnapshotStoreFindsWhatItSaved()
    {
        var store = TimesheetSnapshotStore.NewMemoryTimesheetSnapshotStore();
        var item = TimesheetSnapshot.NewTimesheetSnapshot("code-197", "title-860");
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
        MemoryTimesheetSnapshotStoreFindsWhatItSaved();
    }
}
