namespace Acme.Workforce.Absence.Tests.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LeaveRequestRuleStoreChecks
{
    public static void MemoryLeaveRequestRuleStoreFindsWhatItSaved()
    {
        var store = LeaveRequestRuleStore.NewMemoryLeaveRequestRuleStore();
        var item = LeaveRequestRule.NewLeaveRequestRule("code-782", "label-521");
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
        MemoryLeaveRequestRuleStoreFindsWhatItSaved();
    }
}
