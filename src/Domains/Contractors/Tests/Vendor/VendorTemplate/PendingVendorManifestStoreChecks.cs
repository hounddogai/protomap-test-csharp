namespace Acme.Workforce.Contractors.Tests.Vendor.VendorTemplate;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Checks of vendor template code, run by the project's runner.</summary>
public static class PendingVendorManifestStoreChecks
{
    public static void MemoryPendingVendorManifestStoreFindsWhatItSaved()
    {
        var store = PendingVendorManifestStore.NewMemoryPendingVendorManifestStore();
        var item = PendingVendorManifest.NewPendingVendorManifest("code-304", "label-138");
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
        MemoryPendingVendorManifestStoreFindsWhatItSaved();
    }
}
