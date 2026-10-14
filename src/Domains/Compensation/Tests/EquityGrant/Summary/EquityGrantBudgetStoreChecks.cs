namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantBudgetStoreChecks
{
    public static void MemoryEquityGrantBudgetStoreFindsWhatItSaved()
    {
        var store = EquityGrantBudgetStore.NewMemoryEquityGrantBudgetStore();
        var item = EquityGrantBudget.NewEquityGrantBudget("code-369", "label-18");
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
        MemoryEquityGrantBudgetStoreFindsWhatItSaved();
    }
}
