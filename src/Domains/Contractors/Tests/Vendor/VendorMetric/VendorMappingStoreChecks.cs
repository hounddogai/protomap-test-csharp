namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorMappingStoreChecks
{
    public static void MemoryVendorMappingStoreFindsWhatItSaved()
    {
        var store = VendorMappingStore.NewMemoryVendorMappingStore();
        var item = VendorMapping.NewVendorMapping("code-274", "title-517");
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
        MemoryVendorMappingStoreFindsWhatItSaved();
    }
}
