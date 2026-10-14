namespace Acme.Workforce.Absence.Tests.LeaveRequest.Milestone;

using Acme.Workforce.Absence.LeaveRequest.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LeaveRequestEntryStoreChecks
{
    public static void MemoryLeaveRequestEntryStoreFindsWhatItSaved()
    {
        var store = LeaveRequestEntryStore.NewMemoryLeaveRequestEntryStore();
        var item = LeaveRequestEntry.NewLeaveRequestEntry("code-903", "name-696");
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
        MemoryLeaveRequestEntryStoreFindsWhatItSaved();
    }
}
