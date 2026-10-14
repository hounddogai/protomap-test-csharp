namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyMappingStoreChecks
{
    public static void MemoryWarrantyMappingStoreFindsWhatItSaved()
    {
        var store = WarrantyMappingStore.NewMemoryWarrantyMappingStore();
        var item = WarrantyMapping.NewWarrantyMapping("code-155", "label-791");
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
        MemoryWarrantyMappingStoreFindsWhatItSaved();
    }
}
