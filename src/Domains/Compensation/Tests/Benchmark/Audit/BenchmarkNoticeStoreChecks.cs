namespace Acme.Workforce.Compensation.Tests.Benchmark.Audit;

using Acme.Workforce.Compensation.Benchmark.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BenchmarkNoticeStoreChecks
{
    public static void MemoryBenchmarkNoticeStoreFindsWhatItSaved()
    {
        var store = BenchmarkNoticeStore.NewMemoryBenchmarkNoticeStore();
        var item = BenchmarkNotice.NewBenchmarkNotice("code-470", "title-639");
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
        MemoryBenchmarkNoticeStoreFindsWhatItSaved();
    }
}
