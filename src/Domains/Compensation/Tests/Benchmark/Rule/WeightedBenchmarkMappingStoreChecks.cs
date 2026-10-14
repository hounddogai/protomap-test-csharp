namespace Acme.Workforce.Compensation.Tests.Benchmark.Rule;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class WeightedBenchmarkMappingStoreChecks
{
    public static void MemoryWeightedBenchmarkMappingStoreFindsWhatItSaved()
    {
        var store = WeightedBenchmarkMappingStore.NewMemoryWeightedBenchmarkMappingStore();
        var item = WeightedBenchmarkMapping.NewWeightedBenchmarkMapping("code-565", "title-254");
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
        MemoryWeightedBenchmarkMappingStoreFindsWhatItSaved();
    }
}
