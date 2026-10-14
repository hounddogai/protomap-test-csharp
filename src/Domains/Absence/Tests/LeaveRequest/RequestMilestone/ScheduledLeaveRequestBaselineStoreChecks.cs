namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestBaselineStoreChecks
{
    public static void MemoryScheduledLeaveRequestBaselineStoreFindsWhatItSaved()
    {
        var store = ScheduledLeaveRequestBaselineStore.NewMemoryScheduledLeaveRequestBaselineStore();
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("code-100", "title-97");
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
        MemoryScheduledLeaveRequestBaselineStoreFindsWhatItSaved();
    }
}
