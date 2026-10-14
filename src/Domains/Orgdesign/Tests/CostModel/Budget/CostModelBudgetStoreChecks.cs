namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelBudgetStoreChecks
{
    public static void MemoryCostModelBudgetStoreFindsWhatItSaved()
    {
        var store = CostModelBudgetStore.NewMemoryCostModelBudgetStore();
        var item = CostModelBudget.NewCostModelBudget("code-186", "title-135");
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
        MemoryCostModelBudgetStoreFindsWhatItSaved();
    }
}
