namespace Acme.Workforce.Compensation.Tests.Benchmark.Reservation;

using Acme.Workforce.Compensation.Benchmark.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BenchmarkMilestoneStoreChecks
{
    public static void MemoryBenchmarkMilestoneStoreFindsWhatItSaved()
    {
        var store = BenchmarkMilestoneStore.NewMemoryBenchmarkMilestoneStore();
        var item = BenchmarkMilestone.NewBenchmarkMilestone("code-731", "label-984");
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
        MemoryBenchmarkMilestoneStoreFindsWhatItSaved();
    }
}
