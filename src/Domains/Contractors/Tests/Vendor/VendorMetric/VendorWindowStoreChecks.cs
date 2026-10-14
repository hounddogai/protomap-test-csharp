namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorWindowStoreChecks
{
    public static void MemoryVendorWindowStoreFindsWhatItSaved()
    {
        var store = VendorWindowStore.NewMemoryVendorWindowStore();
        var item = VendorWindow.NewVendorWindow("code-56", "label-667");
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
        MemoryVendorWindowStoreFindsWhatItSaved();
    }
}
