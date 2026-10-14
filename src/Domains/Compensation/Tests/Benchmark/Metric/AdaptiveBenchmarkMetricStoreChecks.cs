namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkMetricStoreChecks
{
    public static void MemoryAdaptiveBenchmarkMetricStoreFindsWhatItSaved()
    {
        var store = AdaptiveBenchmarkMetricStore.NewMemoryAdaptiveBenchmarkMetricStore();
        var item = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric("code-42", "label-686");
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
        MemoryAdaptiveBenchmarkMetricStoreFindsWhatItSaved();
    }
}
