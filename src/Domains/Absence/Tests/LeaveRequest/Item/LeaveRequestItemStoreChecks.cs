namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestItemStoreChecks
{
    public static void MemoryLeaveRequestItemStoreFindsWhatItSaved()
    {
        var store = LeaveRequestItemStore.NewMemoryLeaveRequestItemStore();
        var item = LeaveRequestItem.NewLeaveRequestItem("code-716", "title-327");
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
        MemoryLeaveRequestItemStoreFindsWhatItSaved();
    }
}
