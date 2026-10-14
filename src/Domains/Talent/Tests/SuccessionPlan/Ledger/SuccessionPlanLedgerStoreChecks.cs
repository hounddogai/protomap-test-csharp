namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SuccessionPlanLedgerStoreChecks
{
    public static void MemorySuccessionPlanLedgerStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanLedgerStore.NewMemorySuccessionPlanLedgerStore();
        var item = SuccessionPlanLedger.NewSuccessionPlanLedger("code-967", "title-676");
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
        MemorySuccessionPlanLedgerStoreFindsWhatItSaved();
    }
}
