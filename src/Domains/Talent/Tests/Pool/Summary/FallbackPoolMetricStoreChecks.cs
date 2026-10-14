namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class FallbackPoolMetricStoreChecks
{
    public static void MemoryFallbackPoolMetricStoreFindsWhatItSaved()
    {
        var store = FallbackPoolMetricStore.NewMemoryFallbackPoolMetricStore();
        var item = FallbackPoolMetric.NewFallbackPoolMetric("code-776", "name-246");
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
        MemoryFallbackPoolMetricStoreFindsWhatItSaved();
    }
}
