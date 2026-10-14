namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkBatchStoreChecks
{
    public static void MemoryBenchmarkBatchStoreFindsWhatItSaved()
    {
        var store = BenchmarkBatchStore.NewMemoryBenchmarkBatchStore();
        var item = BenchmarkBatch.NewBenchmarkBatch("code-559", "label-410");
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
        MemoryBenchmarkBatchStoreFindsWhatItSaved();
    }
}
