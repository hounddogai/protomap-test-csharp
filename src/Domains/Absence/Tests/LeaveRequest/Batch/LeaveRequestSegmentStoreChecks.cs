namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class LeaveRequestSegmentStoreChecks
{
    public static void MemoryLeaveRequestSegmentStoreFindsWhatItSaved()
    {
        var store = LeaveRequestSegmentStore.NewMemoryLeaveRequestSegmentStore();
        var item = LeaveRequestSegment.NewLeaveRequestSegment("code-92", "title-733");
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
        MemoryLeaveRequestSegmentStoreFindsWhatItSaved();
    }
}
