namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class RollingLeaveRequestBatchStoreChecks
{
    public static void MemoryRollingLeaveRequestBatchStoreFindsWhatItSaved()
    {
        var store = RollingLeaveRequestBatchStore.NewMemoryRollingLeaveRequestBatchStore();
        var item = RollingLeaveRequestBatch.NewRollingLeaveRequestBatch("code-733", "label-849");
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
        MemoryRollingLeaveRequestBatchStoreFindsWhatItSaved();
    }
}
