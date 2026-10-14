namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestGroupStoreChecks
{
    public static void MemoryLeaveRequestGroupStoreFindsWhatItSaved()
    {
        var store = LeaveRequestGroupStore.NewMemoryLeaveRequestGroupStore();
        var item = LeaveRequestGroup.NewLeaveRequestGroup("code-31", "name-777");
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
        MemoryLeaveRequestGroupStoreFindsWhatItSaved();
    }
}
