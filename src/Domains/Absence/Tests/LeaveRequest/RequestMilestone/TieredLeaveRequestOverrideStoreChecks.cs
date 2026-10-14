namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class TieredLeaveRequestOverrideStoreChecks
{
    public static void MemoryTieredLeaveRequestOverrideStoreFindsWhatItSaved()
    {
        var store = TieredLeaveRequestOverrideStore.NewMemoryTieredLeaveRequestOverrideStore();
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("code-1", "title-262");
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
        MemoryTieredLeaveRequestOverrideStoreFindsWhatItSaved();
    }
}
