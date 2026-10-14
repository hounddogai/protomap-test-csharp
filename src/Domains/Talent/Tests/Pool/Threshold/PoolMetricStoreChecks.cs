namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolMetricStoreChecks
{
    public static void MemoryPoolMetricStoreFindsWhatItSaved()
    {
        var store = PoolMetricStore.NewMemoryPoolMetricStore();
        var item = PoolMetric.NewPoolMetric("code-271", "title-731");
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
        MemoryPoolMetricStoreFindsWhatItSaved();
    }
}
