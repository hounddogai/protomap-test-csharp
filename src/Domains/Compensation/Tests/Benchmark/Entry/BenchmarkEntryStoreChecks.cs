namespace Acme.Workforce.Compensation.Tests.Benchmark.Entry;

using Acme.Workforce.Compensation.Benchmark.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BenchmarkEntryStoreChecks
{
    public static void MemoryBenchmarkEntryStoreFindsWhatItSaved()
    {
        var store = BenchmarkEntryStore.NewMemoryBenchmarkEntryStore();
        var item = BenchmarkEntry.NewBenchmarkEntry("code-909", "title-994");
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
        MemoryBenchmarkEntryStoreFindsWhatItSaved();
    }
}
