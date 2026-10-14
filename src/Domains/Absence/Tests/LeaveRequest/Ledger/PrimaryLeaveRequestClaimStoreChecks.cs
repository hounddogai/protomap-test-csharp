namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestClaimStoreChecks
{
    public static void MemoryPrimaryLeaveRequestClaimStoreFindsWhatItSaved()
    {
        var store = PrimaryLeaveRequestClaimStore.NewMemoryPrimaryLeaveRequestClaimStore();
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("code-429", "name-211");
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
        MemoryPrimaryLeaveRequestClaimStoreFindsWhatItSaved();
    }
}
