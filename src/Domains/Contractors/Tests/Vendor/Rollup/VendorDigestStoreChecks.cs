namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorDigestStoreChecks
{
    public static void MemoryVendorDigestStoreFindsWhatItSaved()
    {
        var store = VendorDigestStore.NewMemoryVendorDigestStore();
        var item = VendorDigest.NewVendorDigest("code-744", "name-942");
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
        MemoryVendorDigestStoreFindsWhatItSaved();
    }
}
