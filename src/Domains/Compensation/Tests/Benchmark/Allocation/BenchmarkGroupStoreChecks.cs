namespace Acme.Workforce.Compensation.Tests.Benchmark.Allocation;

using Acme.Workforce.Compensation.Benchmark.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BenchmarkGroupStoreChecks
{
    public static void MemoryBenchmarkGroupStoreFindsWhatItSaved()
    {
        var store = BenchmarkGroupStore.NewMemoryBenchmarkGroupStore();
        var item = BenchmarkGroup.NewBenchmarkGroup("code-496", "name-669");
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
        MemoryBenchmarkGroupStoreFindsWhatItSaved();
    }
}
