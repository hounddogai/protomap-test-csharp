namespace Acme.Workforce.Compensation.Tests.Benchmark.Proposal;

using Acme.Workforce.Compensation.Benchmark.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredBenchmarkSummaryStoreChecks
{
    public static void MemoryTieredBenchmarkSummaryStoreFindsWhatItSaved()
    {
        var store = TieredBenchmarkSummaryStore.NewMemoryTieredBenchmarkSummaryStore();
        var item = TieredBenchmarkSummary.NewTieredBenchmarkSummary("code-943", "name-766");
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
        MemoryTieredBenchmarkSummaryStoreFindsWhatItSaved();
    }
}
