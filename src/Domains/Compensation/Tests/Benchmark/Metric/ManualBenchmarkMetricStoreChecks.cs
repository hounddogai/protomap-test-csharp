namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualBenchmarkMetricStoreChecks
{
    public static void MemoryManualBenchmarkMetricStoreFindsWhatItSaved()
    {
        var store = ManualBenchmarkMetricStore.NewMemoryManualBenchmarkMetricStore();
        var item = ManualBenchmarkMetric.NewManualBenchmarkMetric("code-971", "label-834");
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
        MemoryManualBenchmarkMetricStoreFindsWhatItSaved();
    }
}
