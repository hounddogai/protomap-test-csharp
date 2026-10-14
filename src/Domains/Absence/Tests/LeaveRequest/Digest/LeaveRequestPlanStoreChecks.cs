namespace Acme.Workforce.Absence.Tests.LeaveRequest.Digest;

using Acme.Workforce.Absence.LeaveRequest.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LeaveRequestPlanStoreChecks
{
    public static void MemoryLeaveRequestPlanStoreFindsWhatItSaved()
    {
        var store = LeaveRequestPlanStore.NewMemoryLeaveRequestPlanStore();
        var item = LeaveRequestPlan.NewLeaveRequestPlan("code-611", "name-401");
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
        MemoryLeaveRequestPlanStoreFindsWhatItSaved();
    }
}
