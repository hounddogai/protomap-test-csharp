namespace Acme.Workforce.Compensation.Tests.Benchmark.Score;

using Acme.Workforce.Compensation.Benchmark.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class TieredBenchmarkScoreStoreChecks
{
    public static void MemoryTieredBenchmarkScoreStoreFindsWhatItSaved()
    {
        var store = TieredBenchmarkScoreStore.NewMemoryTieredBenchmarkScoreStore();
        var item = TieredBenchmarkScore.NewTieredBenchmarkScore("code-598", "name-864");
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
        MemoryTieredBenchmarkScoreStoreFindsWhatItSaved();
    }
}
