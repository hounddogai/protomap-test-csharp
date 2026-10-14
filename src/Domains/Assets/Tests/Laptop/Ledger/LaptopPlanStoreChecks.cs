namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopPlanStoreChecks
{
    public static void MemoryLaptopPlanStoreFindsWhatItSaved()
    {
        var store = LaptopPlanStore.NewMemoryLaptopPlanStore();
        var item = LaptopPlan.NewLaptopPlan("code-678", "name-144");
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
        MemoryLaptopPlanStoreFindsWhatItSaved();
    }
}
