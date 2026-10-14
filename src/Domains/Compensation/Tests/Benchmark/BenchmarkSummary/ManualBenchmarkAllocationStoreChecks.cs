namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class ManualBenchmarkAllocationStoreChecks
{
    public static void MemoryManualBenchmarkAllocationStoreFindsWhatItSaved()
    {
        var store = ManualBenchmarkAllocationStore.NewMemoryManualBenchmarkAllocationStore();
        var item = ManualBenchmarkAllocation.NewManualBenchmarkAllocation("code-36", "label-351");
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
        MemoryManualBenchmarkAllocationStoreFindsWhatItSaved();
    }
}
