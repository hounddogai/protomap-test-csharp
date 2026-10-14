namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingBenchmarkSummaryStoreChecks
{
    public static void MemoryRollingBenchmarkSummaryStoreFindsWhatItSaved()
    {
        var store = RollingBenchmarkSummaryStore.NewMemoryRollingBenchmarkSummaryStore();
        var item = RollingBenchmarkSummary.NewRollingBenchmarkSummary("code-782", "title-76");
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
        MemoryRollingBenchmarkSummaryStoreFindsWhatItSaved();
    }
}
