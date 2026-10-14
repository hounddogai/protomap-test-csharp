namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LeaveRequestScheduleStoreChecks
{
    public static void MemoryLeaveRequestScheduleStoreFindsWhatItSaved()
    {
        var store = LeaveRequestScheduleStore.NewMemoryLeaveRequestScheduleStore();
        var item = LeaveRequestSchedule.NewLeaveRequestSchedule("code-293", "title-713");
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
        MemoryLeaveRequestScheduleStoreFindsWhatItSaved();
    }
}
