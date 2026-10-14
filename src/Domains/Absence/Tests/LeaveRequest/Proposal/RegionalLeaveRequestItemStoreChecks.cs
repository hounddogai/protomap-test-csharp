namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RegionalLeaveRequestItemStoreChecks
{
    public static void MemoryRegionalLeaveRequestItemStoreFindsWhatItSaved()
    {
        var store = RegionalLeaveRequestItemStore.NewMemoryRegionalLeaveRequestItemStore();
        var item = RegionalLeaveRequestItem.NewRegionalLeaveRequestItem("code-739", "label-952");
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
        MemoryRegionalLeaveRequestItemStoreFindsWhatItSaved();
    }
}
