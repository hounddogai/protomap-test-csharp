namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkEntry;

using Acme.Workforce.Compensation.Benchmark.BenchmarkEntry;

/// <summary>Checks of benchmark entry code, run by the project's runner.</summary>
public static class LenientBenchmarkAuditStoreChecks
{
    public static void MemoryLenientBenchmarkAuditStoreFindsWhatItSaved()
    {
        var store = LenientBenchmarkAuditStore.NewMemoryLenientBenchmarkAuditStore();
        var item = LenientBenchmarkAudit.NewLenientBenchmarkAudit("code-716", "title-308");
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
        MemoryLenientBenchmarkAuditStoreFindsWhatItSaved();
    }
}
