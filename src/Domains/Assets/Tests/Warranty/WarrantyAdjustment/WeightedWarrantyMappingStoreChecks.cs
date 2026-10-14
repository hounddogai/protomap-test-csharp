namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class WeightedWarrantyMappingStoreChecks
{
    public static void MemoryWeightedWarrantyMappingStoreFindsWhatItSaved()
    {
        var store = WeightedWarrantyMappingStore.NewMemoryWeightedWarrantyMappingStore();
        var item = WeightedWarrantyMapping.NewWeightedWarrantyMapping("code-496", "label-839");
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
        MemoryWeightedWarrantyMappingStoreFindsWhatItSaved();
    }
}
