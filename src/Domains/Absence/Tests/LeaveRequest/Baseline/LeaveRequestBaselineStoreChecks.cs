namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestBaselineStoreChecks
{
    public static void MemoryLeaveRequestBaselineStoreFindsWhatItSaved()
    {
        var store = LeaveRequestBaselineStore.NewMemoryLeaveRequestBaselineStore();
        var item = LeaveRequestBaseline.NewLeaveRequestBaseline("code-811", "title-360");
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
        MemoryLeaveRequestBaselineStoreFindsWhatItSaved();
    }
}
