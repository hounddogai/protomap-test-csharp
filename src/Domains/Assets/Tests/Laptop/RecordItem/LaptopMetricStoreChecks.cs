namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopMetricStoreChecks
{
    public static void MemoryLaptopMetricStoreFindsWhatItSaved()
    {
        var store = LaptopMetricStore.NewMemoryLaptopMetricStore();
        var item = LaptopMetric.NewLaptopMetric("code-46", "label-739");
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
        MemoryLaptopMetricStoreFindsWhatItSaved();
    }
}
