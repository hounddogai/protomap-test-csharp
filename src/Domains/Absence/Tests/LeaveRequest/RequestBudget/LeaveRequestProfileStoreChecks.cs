namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestBudget;

using Acme.Workforce.Absence.LeaveRequest.RequestBudget;

/// <summary>Checks of request budget code, run by the project's runner.</summary>
public static class LeaveRequestProfileStoreChecks
{
    public static void MemoryLeaveRequestProfileStoreFindsWhatItSaved()
    {
        var store = LeaveRequestProfileStore.NewMemoryLeaveRequestProfileStore();
        var item = LeaveRequestProfile.NewLeaveRequestProfile("code-637", "label-396");
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
        MemoryLeaveRequestProfileStoreFindsWhatItSaved();
    }
}
