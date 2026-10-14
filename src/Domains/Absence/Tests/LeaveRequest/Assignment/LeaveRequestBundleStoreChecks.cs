namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LeaveRequestBundleStoreChecks
{
    public static void MemoryLeaveRequestBundleStoreFindsWhatItSaved()
    {
        var store = LeaveRequestBundleStore.NewMemoryLeaveRequestBundleStore();
        var item = LeaveRequestBundle.NewLeaveRequestBundle("code-774", "name-655");
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
        MemoryLeaveRequestBundleStoreFindsWhatItSaved();
    }
}
