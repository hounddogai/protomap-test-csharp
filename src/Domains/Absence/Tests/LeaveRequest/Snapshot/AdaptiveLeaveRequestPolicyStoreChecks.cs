namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AdaptiveLeaveRequestPolicyStoreChecks
{
    public static void MemoryAdaptiveLeaveRequestPolicyStoreFindsWhatItSaved()
    {
        var store = AdaptiveLeaveRequestPolicyStore.NewMemoryAdaptiveLeaveRequestPolicyStore();
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("code-562", "name-723");
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
        MemoryAdaptiveLeaveRequestPolicyStoreFindsWhatItSaved();
    }
}
