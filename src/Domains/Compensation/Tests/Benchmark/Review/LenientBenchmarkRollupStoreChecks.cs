namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBenchmarkRollupStoreChecks
{
    public static void MemoryLenientBenchmarkRollupStoreFindsWhatItSaved()
    {
        var store = LenientBenchmarkRollupStore.NewMemoryLenientBenchmarkRollupStore();
        var item = LenientBenchmarkRollup.NewLenientBenchmarkRollup("code-452", "title-11");
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
        MemoryLenientBenchmarkRollupStoreFindsWhatItSaved();
    }
}
