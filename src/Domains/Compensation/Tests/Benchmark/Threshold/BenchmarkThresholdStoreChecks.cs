namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkThresholdStoreChecks
{
    public static void MemoryBenchmarkThresholdStoreFindsWhatItSaved()
    {
        var store = BenchmarkThresholdStore.NewMemoryBenchmarkThresholdStore();
        var item = BenchmarkThreshold.NewBenchmarkThreshold("code-628", "label-144");
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
        MemoryBenchmarkThresholdStoreFindsWhatItSaved();
    }
}
