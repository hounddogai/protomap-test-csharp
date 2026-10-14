namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanLedgerStoreChecks
{
    public static void MemoryReturnPlanLedgerStoreFindsWhatItSaved()
    {
        var store = ReturnPlanLedgerStore.NewMemoryReturnPlanLedgerStore();
        var item = ReturnPlanLedger.NewReturnPlanLedger("code-792", "title-100");
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
        MemoryReturnPlanLedgerStoreFindsWhatItSaved();
    }
}
