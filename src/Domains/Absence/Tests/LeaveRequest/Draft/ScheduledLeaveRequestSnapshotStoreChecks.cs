namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestSnapshotStoreChecks
{
    public static void MemoryScheduledLeaveRequestSnapshotStoreFindsWhatItSaved()
    {
        var store = ScheduledLeaveRequestSnapshotStore.NewMemoryScheduledLeaveRequestSnapshotStore();
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("code-12", "name-996");
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
        MemoryScheduledLeaveRequestSnapshotStoreFindsWhatItSaved();
    }
}
