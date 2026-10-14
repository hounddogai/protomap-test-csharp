namespace Acme.Workforce.Compensation.Tests.Benchmark.Rollup;

using Acme.Workforce.Compensation.Benchmark.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualBenchmarkWindowStoreChecks
{
    public static void MemoryManualBenchmarkWindowStoreFindsWhatItSaved()
    {
        var store = ManualBenchmarkWindowStore.NewMemoryManualBenchmarkWindowStore();
        var item = ManualBenchmarkWindow.NewManualBenchmarkWindow("code-892", "title-712");
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
        MemoryManualBenchmarkWindowStoreFindsWhatItSaved();
    }
}
