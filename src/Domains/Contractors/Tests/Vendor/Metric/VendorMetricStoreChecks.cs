namespace Acme.Workforce.Contractors.Tests.Vendor.Metric;

using Acme.Workforce.Contractors.Vendor.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class VendorMetricStoreChecks
{
    public static void MemoryVendorMetricStoreFindsWhatItSaved()
    {
        var store = VendorMetricStore.NewMemoryVendorMetricStore();
        var item = VendorMetric.NewVendorMetric("code-305", "label-944");
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
        MemoryVendorMetricStoreFindsWhatItSaved();
    }
}
