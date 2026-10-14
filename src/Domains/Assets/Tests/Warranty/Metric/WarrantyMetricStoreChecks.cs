namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyMetricStoreChecks
{
    public static void MemoryWarrantyMetricStoreFindsWhatItSaved()
    {
        var store = WarrantyMetricStore.NewMemoryWarrantyMetricStore();
        var item = WarrantyMetric.NewWarrantyMetric("code-625", "title-11");
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
        MemoryWarrantyMetricStoreFindsWhatItSaved();
    }
}
