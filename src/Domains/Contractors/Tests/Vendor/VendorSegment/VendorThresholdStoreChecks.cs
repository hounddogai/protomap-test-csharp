namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorThresholdStoreChecks
{
    public static void MemoryVendorThresholdStoreFindsWhatItSaved()
    {
        var store = VendorThresholdStore.NewMemoryVendorThresholdStore();
        var item = VendorThreshold.NewVendorThreshold("code-603", "name-36");
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
        MemoryVendorThresholdStoreFindsWhatItSaved();
    }
}
