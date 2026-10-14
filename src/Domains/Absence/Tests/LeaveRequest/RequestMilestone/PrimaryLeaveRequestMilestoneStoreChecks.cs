namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestMilestoneStoreChecks
{
    public static void MemoryPrimaryLeaveRequestMilestoneStoreFindsWhatItSaved()
    {
        var store = PrimaryLeaveRequestMilestoneStore.NewMemoryPrimaryLeaveRequestMilestoneStore();
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("code-82", "title-385");
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
        MemoryPrimaryLeaveRequestMilestoneStoreFindsWhatItSaved();
    }
}
