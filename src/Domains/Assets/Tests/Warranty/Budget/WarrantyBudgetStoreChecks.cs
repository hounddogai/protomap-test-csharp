namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class WarrantyBudgetStoreChecks
{
    public static void MemoryWarrantyBudgetStoreFindsWhatItSaved()
    {
        var store = WarrantyBudgetStore.NewMemoryWarrantyBudgetStore();
        var item = WarrantyBudget.NewWarrantyBudget("code-394", "name-262");
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
        MemoryWarrantyBudgetStoreFindsWhatItSaved();
    }
}
