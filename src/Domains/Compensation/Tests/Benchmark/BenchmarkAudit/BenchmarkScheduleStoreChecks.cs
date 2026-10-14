namespace Acme.Workforce.Compensation.Tests.Benchmark.BenchmarkAudit;

using Acme.Workforce.Compensation.Benchmark.BenchmarkAudit;

/// <summary>Checks of benchmark audit code, run by the project's runner.</summary>
public static class BenchmarkScheduleStoreChecks
{
    public static void MemoryBenchmarkScheduleStoreFindsWhatItSaved()
    {
        var store = BenchmarkScheduleStore.NewMemoryBenchmarkScheduleStore();
        var item = BenchmarkSchedule.NewBenchmarkSchedule("code-543", "label-757");
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
        MemoryBenchmarkScheduleStoreFindsWhatItSaved();
    }
}
