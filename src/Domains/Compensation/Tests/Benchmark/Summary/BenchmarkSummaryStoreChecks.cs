namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkSummaryStoreChecks
{
    public static void MemoryBenchmarkSummaryStoreFindsWhatItSaved()
    {
        var store = BenchmarkSummaryStore.NewMemoryBenchmarkSummaryStore();
        var item = BenchmarkSummary.NewBenchmarkSummary("code-520", "label-137");
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
        MemoryBenchmarkSummaryStoreFindsWhatItSaved();
    }
}
