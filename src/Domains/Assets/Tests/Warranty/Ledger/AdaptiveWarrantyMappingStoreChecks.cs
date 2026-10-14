namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMappingStoreChecks
{
    public static void MemoryAdaptiveWarrantyMappingStoreFindsWhatItSaved()
    {
        var store = AdaptiveWarrantyMappingStore.NewMemoryAdaptiveWarrantyMappingStore();
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("code-248", "label-211");
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
        MemoryAdaptiveWarrantyMappingStoreFindsWhatItSaved();
    }
}
