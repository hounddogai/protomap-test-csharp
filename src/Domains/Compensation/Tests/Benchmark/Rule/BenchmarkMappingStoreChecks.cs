namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BenchmarkMappingStoreChecks
{
    public static void MemoryBenchmarkMappingStoreFindsWhatItSaved()
    {
        var store = BenchmarkMappingStore.NewMemoryBenchmarkMappingStore();
        var item = BenchmarkMapping.NewBenchmarkMapping("code-594", "name-416");
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
        MemoryBenchmarkMappingStoreFindsWhatItSaved();
    }
}
