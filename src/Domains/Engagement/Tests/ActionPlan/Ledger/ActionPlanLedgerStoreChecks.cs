namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ActionPlanLedgerStoreChecks
{
    public static void MemoryActionPlanLedgerStoreFindsWhatItSaved()
    {
        var store = ActionPlanLedgerStore.NewMemoryActionPlanLedgerStore();
        var item = ActionPlanLedger.NewActionPlanLedger("code-982", "title-662");
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
        MemoryActionPlanLedgerStoreFindsWhatItSaved();
    }
}
