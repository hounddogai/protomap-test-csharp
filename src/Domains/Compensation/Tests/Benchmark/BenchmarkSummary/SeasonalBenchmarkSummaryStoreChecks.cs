namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class SeasonalBenchmarkSummaryStoreChecks
{
    public static void MemorySeasonalBenchmarkSummaryStoreFindsWhatItSaved()
    {
        var store = SeasonalBenchmarkSummaryStore.NewMemorySeasonalBenchmarkSummaryStore();
        var item = SeasonalBenchmarkSummary.NewSeasonalBenchmarkSummary("code-378", "name-674");
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
        MemorySeasonalBenchmarkSummaryStoreFindsWhatItSaved();
    }
}
