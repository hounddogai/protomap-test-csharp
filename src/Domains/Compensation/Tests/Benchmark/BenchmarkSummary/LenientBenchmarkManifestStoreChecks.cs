namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkSummary;

using Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

/// <summary>Checks of benchmark summary code, run by the project's runner.</summary>
public static class LenientBenchmarkManifestStoreChecks
{
    public static void MemoryLenientBenchmarkManifestStoreFindsWhatItSaved()
    {
        var store = LenientBenchmarkManifestStore.NewMemoryLenientBenchmarkManifestStore();
        var item = LenientBenchmarkManifest.NewLenientBenchmarkManifest("code-383", "name-521");
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
        MemoryLenientBenchmarkManifestStoreFindsWhatItSaved();
    }
}
