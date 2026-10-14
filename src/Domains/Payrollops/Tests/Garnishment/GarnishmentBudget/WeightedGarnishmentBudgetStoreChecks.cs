namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

/// <summary>Checks of garnishment budget code, run by the project's runner.</summary>
public static class WeightedGarnishmentBudgetStoreChecks
{
    public static void MemoryWeightedGarnishmentBudgetStoreFindsWhatItSaved()
    {
        var store = WeightedGarnishmentBudgetStore.NewMemoryWeightedGarnishmentBudgetStore();
        var item = WeightedGarnishmentBudget.NewWeightedGarnishmentBudget("code-706", "name-711");
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
        MemoryWeightedGarnishmentBudgetStoreFindsWhatItSaved();
    }
}
