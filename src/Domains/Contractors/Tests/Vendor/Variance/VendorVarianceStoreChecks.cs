namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorVarianceStoreChecks
{
    public static void MemoryVendorVarianceStoreFindsWhatItSaved()
    {
        var store = VendorVarianceStore.NewMemoryVendorVarianceStore();
        var item = VendorVariance.NewVendorVariance("code-678", "title-206");
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
        MemoryVendorVarianceStoreFindsWhatItSaved();
    }
}
