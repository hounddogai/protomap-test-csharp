namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class GlobalBenchmarkOverrideStoreChecks
{
    public static void MemoryGlobalBenchmarkOverrideStoreFindsWhatItSaved()
    {
        var store = GlobalBenchmarkOverrideStore.NewMemoryGlobalBenchmarkOverrideStore();
        var item = GlobalBenchmarkOverride.NewGlobalBenchmarkOverride("code-758", "label-28");
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
        MemoryGlobalBenchmarkOverrideStoreFindsWhatItSaved();
    }
}
