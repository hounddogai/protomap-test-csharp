namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestBudgetStoreChecks
{
    public static void MemoryPrimaryLeaveRequestBudgetStoreFindsWhatItSaved()
    {
        var store = PrimaryLeaveRequestBudgetStore.NewMemoryPrimaryLeaveRequestBudgetStore();
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("code-112", "title-819");
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
        MemoryPrimaryLeaveRequestBudgetStoreFindsWhatItSaved();
    }
}
