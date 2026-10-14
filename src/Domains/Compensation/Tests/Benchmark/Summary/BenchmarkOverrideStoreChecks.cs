namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkOverrideStoreChecks
{
    public static void MemoryBenchmarkOverrideStoreFindsWhatItSaved()
    {
        var store = BenchmarkOverrideStore.NewMemoryBenchmarkOverrideStore();
        var item = BenchmarkOverride.NewBenchmarkOverride("code-331", "title-341");
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
        MemoryBenchmarkOverrideStoreFindsWhatItSaved();
    }
}
