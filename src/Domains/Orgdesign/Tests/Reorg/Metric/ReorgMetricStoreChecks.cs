namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMetricStoreChecks
{
    public static void MemoryReorgMetricStoreFindsWhatItSaved()
    {
        var store = ReorgMetricStore.NewMemoryReorgMetricStore();
        var item = ReorgMetric.NewReorgMetric("code-772", "title-38");
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
        MemoryReorgMetricStoreFindsWhatItSaved();
    }
}
