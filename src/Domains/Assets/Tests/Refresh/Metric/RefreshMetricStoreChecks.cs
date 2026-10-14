namespace Acme.Workforce.Assets.Tests.Refresh.Metric;

using Acme.Workforce.Assets.Refresh.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RefreshMetricStoreChecks
{
    public static void MemoryRefreshMetricStoreFindsWhatItSaved()
    {
        var store = RefreshMetricStore.NewMemoryRefreshMetricStore();
        var item = RefreshMetric.NewRefreshMetric("code-871", "name-310");
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
        MemoryRefreshMetricStoreFindsWhatItSaved();
    }
}
