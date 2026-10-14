namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>Checks of benchmark adjustment code, run by the project's runner.</summary>
public static class TieredBenchmarkAdjustmentStoreChecks
{
    public static void MemoryTieredBenchmarkAdjustmentStoreFindsWhatItSaved()
    {
        var store = TieredBenchmarkAdjustmentStore.NewMemoryTieredBenchmarkAdjustmentStore();
        var item = TieredBenchmarkAdjustment.NewTieredBenchmarkAdjustment("code-659", "name-811");
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
        MemoryTieredBenchmarkAdjustmentStoreFindsWhatItSaved();
    }
}
