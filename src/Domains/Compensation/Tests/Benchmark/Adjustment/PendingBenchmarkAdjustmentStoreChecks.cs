namespace Acme.Workforce.Compensation.Tests.Benchmark.Adjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingBenchmarkAdjustmentStoreChecks
{
    public static void MemoryPendingBenchmarkAdjustmentStoreFindsWhatItSaved()
    {
        var store = PendingBenchmarkAdjustmentStore.NewMemoryPendingBenchmarkAdjustmentStore();
        var item = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment("code-760", "title-586");
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
        MemoryPendingBenchmarkAdjustmentStoreFindsWhatItSaved();
    }
}
