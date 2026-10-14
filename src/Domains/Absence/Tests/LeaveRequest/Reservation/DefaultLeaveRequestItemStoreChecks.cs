namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultLeaveRequestItemStoreChecks
{
    public static void MemoryDefaultLeaveRequestItemStoreFindsWhatItSaved()
    {
        var store = DefaultLeaveRequestItemStore.NewMemoryDefaultLeaveRequestItemStore();
        var item = DefaultLeaveRequestItem.NewDefaultLeaveRequestItem("code-401", "title-451");
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
        MemoryDefaultLeaveRequestItemStoreFindsWhatItSaved();
    }
}
