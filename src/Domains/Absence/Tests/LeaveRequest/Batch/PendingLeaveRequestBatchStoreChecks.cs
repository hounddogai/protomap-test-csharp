namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingLeaveRequestBatchStoreChecks
{
    public static void MemoryPendingLeaveRequestBatchStoreFindsWhatItSaved()
    {
        var store = PendingLeaveRequestBatchStore.NewMemoryPendingLeaveRequestBatchStore();
        var item = PendingLeaveRequestBatch.NewPendingLeaveRequestBatch("code-711", "title-999");
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
        MemoryPendingLeaveRequestBatchStoreFindsWhatItSaved();
    }
}
