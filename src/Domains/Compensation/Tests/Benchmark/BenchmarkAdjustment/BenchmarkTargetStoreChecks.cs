namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

/// <summary>Checks of benchmark adjustment code, run by the project's runner.</summary>
public static class BenchmarkTargetStoreChecks
{
    public static void MemoryBenchmarkTargetStoreFindsWhatItSaved()
    {
        var store = BenchmarkTargetStore.NewMemoryBenchmarkTargetStore();
        var item = BenchmarkTarget.NewBenchmarkTarget("code-264", "name-2");
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
        MemoryBenchmarkTargetStoreFindsWhatItSaved();
    }
}
