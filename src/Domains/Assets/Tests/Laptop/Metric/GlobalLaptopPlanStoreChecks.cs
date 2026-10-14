namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class GlobalLaptopPlanStoreChecks
{
    public static void MemoryGlobalLaptopPlanStoreFindsWhatItSaved()
    {
        var store = GlobalLaptopPlanStore.NewMemoryGlobalLaptopPlanStore();
        var item = GlobalLaptopPlan.NewGlobalLaptopPlan("code-804", "label-32");
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
        MemoryGlobalLaptopPlanStoreFindsWhatItSaved();
    }
}
