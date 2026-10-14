namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class BenchmarkWindowStoreChecks
{
    public static void MemoryBenchmarkWindowStoreFindsWhatItSaved()
    {
        var store = BenchmarkWindowStore.NewMemoryBenchmarkWindowStore();
        var item = BenchmarkWindow.NewBenchmarkWindow("code-866", "name-287");
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
        MemoryBenchmarkWindowStoreFindsWhatItSaved();
    }
}
