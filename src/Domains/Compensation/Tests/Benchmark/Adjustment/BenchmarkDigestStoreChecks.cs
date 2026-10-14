namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class BenchmarkDigestStoreChecks
{
    public static void MemoryBenchmarkDigestStoreFindsWhatItSaved()
    {
        var store = BenchmarkDigestStore.NewMemoryBenchmarkDigestStore();
        var item = BenchmarkDigest.NewBenchmarkDigest("code-48", "title-828");
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
        MemoryBenchmarkDigestStoreFindsWhatItSaved();
    }
}
