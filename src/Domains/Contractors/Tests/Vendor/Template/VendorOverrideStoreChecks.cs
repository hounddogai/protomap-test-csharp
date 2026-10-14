namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorOverrideStoreChecks
{
    public static void MemoryVendorOverrideStoreFindsWhatItSaved()
    {
        var store = VendorOverrideStore.NewMemoryVendorOverrideStore();
        var item = VendorOverride.NewVendorOverride("code-841", "label-377");
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
        MemoryVendorOverrideStoreFindsWhatItSaved();
    }
}
