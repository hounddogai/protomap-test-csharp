namespace Acme.Workforce.Compensation.Tests.Benchmark.Profile;

using Acme.Workforce.Compensation.Benchmark.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class BenchmarkSnapshotStoreChecks
{
    public static void MemoryBenchmarkSnapshotStoreFindsWhatItSaved()
    {
        var store = BenchmarkSnapshotStore.NewMemoryBenchmarkSnapshotStore();
        var item = BenchmarkSnapshot.NewBenchmarkSnapshot("code-122", "name-445");
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
        MemoryBenchmarkSnapshotStoreFindsWhatItSaved();
    }
}
