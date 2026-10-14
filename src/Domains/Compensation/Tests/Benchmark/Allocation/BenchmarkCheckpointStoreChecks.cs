namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkCheckpointStoreChecks
{
    public static void MemoryBenchmarkCheckpointStoreFindsWhatItSaved()
    {
        var store = BenchmarkCheckpointStore.NewMemoryBenchmarkCheckpointStore();
        var item = BenchmarkCheckpoint.NewBenchmarkCheckpoint("code-669", "name-422");
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
        MemoryBenchmarkCheckpointStoreFindsWhatItSaved();
    }
}
