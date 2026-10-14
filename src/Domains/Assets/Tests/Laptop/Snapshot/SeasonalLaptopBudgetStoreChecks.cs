namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalLaptopBudgetStoreChecks
{
    public static void MemorySeasonalLaptopBudgetStoreFindsWhatItSaved()
    {
        var store = SeasonalLaptopBudgetStore.NewMemorySeasonalLaptopBudgetStore();
        var item = SeasonalLaptopBudget.NewSeasonalLaptopBudget("code-887", "name-748");
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
        MemorySeasonalLaptopBudgetStoreFindsWhatItSaved();
    }
}
