namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class WeightedLaptopEstimateStoreChecks
{
    public static void MemoryWeightedLaptopEstimateStoreFindsWhatItSaved()
    {
        var store = WeightedLaptopEstimateStore.NewMemoryWeightedLaptopEstimateStore();
        var item = WeightedLaptopEstimate.NewWeightedLaptopEstimate("code-152", "title-599");
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
        MemoryWeightedLaptopEstimateStoreFindsWhatItSaved();
    }
}
