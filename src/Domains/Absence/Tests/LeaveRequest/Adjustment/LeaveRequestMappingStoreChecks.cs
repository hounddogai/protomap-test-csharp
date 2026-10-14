namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestMappingStoreChecks
{
    public static void MemoryLeaveRequestMappingStoreFindsWhatItSaved()
    {
        var store = LeaveRequestMappingStore.NewMemoryLeaveRequestMappingStore();
        var item = LeaveRequestMapping.NewLeaveRequestMapping("code-798", "title-767");
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
        MemoryLeaveRequestMappingStoreFindsWhatItSaved();
    }
}
