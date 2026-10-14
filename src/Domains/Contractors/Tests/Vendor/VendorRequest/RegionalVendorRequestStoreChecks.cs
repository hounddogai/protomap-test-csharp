namespace Acme.Workforce.Contractors.Tests.Vendor.VendorRequest;

using Acme.Workforce.Contractors.Vendor.VendorRequest;

/// <summary>Checks of vendor request code, run by the project's runner.</summary>
public static class RegionalVendorRequestStoreChecks
{
    public static void MemoryRegionalVendorRequestStoreFindsWhatItSaved()
    {
        var store = RegionalVendorRequestStore.NewMemoryRegionalVendorRequestStore();
        var item = RegionalVendorRequest.NewRegionalVendorRequest("code-372", "title-6");
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
        MemoryRegionalVendorRequestStoreFindsWhatItSaved();
    }
}
