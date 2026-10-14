namespace Acme.Workforce.Contractors.Tests.Vendor.Segment;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class VendorQuotaStoreChecks
{
    public static void MemoryVendorQuotaStoreFindsWhatItSaved()
    {
        var store = VendorQuotaStore.NewMemoryVendorQuotaStore();
        var item = VendorQuota.NewVendorQuota("code-599", "title-948");
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
        MemoryVendorQuotaStoreFindsWhatItSaved();
    }
}
