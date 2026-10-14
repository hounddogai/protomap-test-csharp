namespace Acme.Workforce.Compensation.Tests.Benchmark.Metric;

using Acme.Workforce.Compensation.Benchmark.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RegionalBenchmarkEstimateStoreChecks
{
    public static void MemoryRegionalBenchmarkEstimateStoreFindsWhatItSaved()
    {
        var store = RegionalBenchmarkEstimateStore.NewMemoryRegionalBenchmarkEstimateStore();
        var item = RegionalBenchmarkEstimate.NewRegionalBenchmarkEstimate("code-678", "name-841");
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
        MemoryRegionalBenchmarkEstimateStoreFindsWhatItSaved();
    }
}
