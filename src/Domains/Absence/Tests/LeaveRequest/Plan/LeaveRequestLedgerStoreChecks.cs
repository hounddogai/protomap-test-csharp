namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LeaveRequestLedgerStoreChecks
{
    public static void MemoryLeaveRequestLedgerStoreFindsWhatItSaved()
    {
        var store = LeaveRequestLedgerStore.NewMemoryLeaveRequestLedgerStore();
        var item = LeaveRequestLedger.NewLeaveRequestLedger("code-890", "name-275");
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
        MemoryLeaveRequestLedgerStoreFindsWhatItSaved();
    }
}
