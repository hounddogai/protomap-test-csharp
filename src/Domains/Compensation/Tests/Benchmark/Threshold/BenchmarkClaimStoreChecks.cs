namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkClaimStoreChecks
{
    public static void MemoryBenchmarkClaimStoreFindsWhatItSaved()
    {
        var store = BenchmarkClaimStore.NewMemoryBenchmarkClaimStore();
        var item = BenchmarkClaim.NewBenchmarkClaim("code-475", "name-807");
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
        MemoryBenchmarkClaimStoreFindsWhatItSaved();
    }
}
