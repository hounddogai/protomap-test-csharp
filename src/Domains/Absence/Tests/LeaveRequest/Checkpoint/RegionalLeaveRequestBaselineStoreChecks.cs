namespace Acme.Workforce.Absence.Tests.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalLeaveRequestBaselineStoreChecks
{
    public static void MemoryRegionalLeaveRequestBaselineStoreFindsWhatItSaved()
    {
        var store = RegionalLeaveRequestBaselineStore.NewMemoryRegionalLeaveRequestBaselineStore();
        var item = RegionalLeaveRequestBaseline.NewRegionalLeaveRequestBaseline("code-765", "name-34");
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
        MemoryRegionalLeaveRequestBaselineStoreFindsWhatItSaved();
    }
}
