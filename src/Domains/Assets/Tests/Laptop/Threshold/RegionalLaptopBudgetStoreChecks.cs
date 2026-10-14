namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RegionalLaptopBudgetStoreChecks
{
    public static void MemoryRegionalLaptopBudgetStoreFindsWhatItSaved()
    {
        var store = RegionalLaptopBudgetStore.NewMemoryRegionalLaptopBudgetStore();
        var item = RegionalLaptopBudget.NewRegionalLaptopBudget("code-372", "name-57");
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
        MemoryRegionalLaptopBudgetStoreFindsWhatItSaved();
    }
}
