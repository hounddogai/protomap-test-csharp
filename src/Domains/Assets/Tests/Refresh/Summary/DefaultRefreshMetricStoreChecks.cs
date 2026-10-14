namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultRefreshMetricStoreChecks
{
    public static void MemoryDefaultRefreshMetricStoreFindsWhatItSaved()
    {
        var store = DefaultRefreshMetricStore.NewMemoryDefaultRefreshMetricStore();
        var item = DefaultRefreshMetric.NewDefaultRefreshMetric("code-607", "name-959");
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
        MemoryDefaultRefreshMetricStoreFindsWhatItSaved();
    }
}
