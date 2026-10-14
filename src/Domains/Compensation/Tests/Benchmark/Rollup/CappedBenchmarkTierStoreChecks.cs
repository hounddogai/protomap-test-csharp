namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class CappedBenchmarkTierStoreChecks
{
    public static void MemoryCappedBenchmarkTierStoreFindsWhatItSaved()
    {
        var store = CappedBenchmarkTierStore.NewMemoryCappedBenchmarkTierStore();
        var item = CappedBenchmarkTier.NewCappedBenchmarkTier("code-809", "label-3");
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
        MemoryCappedBenchmarkTierStoreFindsWhatItSaved();
    }
}
