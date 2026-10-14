namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestAdjustmentStoreChecks
{
    public static void MemoryLeaveRequestAdjustmentStoreFindsWhatItSaved()
    {
        var store = LeaveRequestAdjustmentStore.NewMemoryLeaveRequestAdjustmentStore();
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("code-117", "title-792");
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
        MemoryLeaveRequestAdjustmentStoreFindsWhatItSaved();
    }
}
