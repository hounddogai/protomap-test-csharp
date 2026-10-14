namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorAdjustmentStoreChecks
{
    public static void MemoryVendorAdjustmentStoreFindsWhatItSaved()
    {
        var store = VendorAdjustmentStore.NewMemoryVendorAdjustmentStore();
        var item = VendorAdjustment.NewVendorAdjustment("code-403", "title-620");
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
        MemoryVendorAdjustmentStoreFindsWhatItSaved();
    }
}
