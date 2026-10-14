namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestTierStoreChecks
{
    public static void MemoryLeaveRequestTierStoreFindsWhatItSaved()
    {
        var store = LeaveRequestTierStore.NewMemoryLeaveRequestTierStore();
        var item = LeaveRequestTier.NewLeaveRequestTier("code-991", "label-294");
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
        MemoryLeaveRequestTierStoreFindsWhatItSaved();
    }
}
