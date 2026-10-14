namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkReviewStoreChecks
{
    public static void MemoryBenchmarkReviewStoreFindsWhatItSaved()
    {
        var store = BenchmarkReviewStore.NewMemoryBenchmarkReviewStore();
        var item = BenchmarkReview.NewBenchmarkReview("code-397", "label-744");
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
        MemoryBenchmarkReviewStoreFindsWhatItSaved();
    }
}
