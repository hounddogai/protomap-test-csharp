namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestRollupStoreChecks
{
    public static void MemoryScheduledLeaveRequestRollupStoreFindsWhatItSaved()
    {
        var store = ScheduledLeaveRequestRollupStore.NewMemoryScheduledLeaveRequestRollupStore();
        var item = ScheduledLeaveRequestRollup.NewScheduledLeaveRequestRollup("code-521", "label-500");
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
        MemoryScheduledLeaveRequestRollupStoreFindsWhatItSaved();
    }
}
