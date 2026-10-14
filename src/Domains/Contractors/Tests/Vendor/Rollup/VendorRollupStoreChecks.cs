namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorRollupStoreChecks
{
    public static void MemoryVendorRollupStoreFindsWhatItSaved()
    {
        var store = VendorRollupStore.NewMemoryVendorRollupStore();
        var item = VendorRollup.NewVendorRollup("code-713", "title-960");
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
        MemoryVendorRollupStoreFindsWhatItSaved();
    }
}
