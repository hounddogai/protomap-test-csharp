namespace Acme.Workforce.Compensation.Tests.Benchmark.Milestone;

using Acme.Workforce.Compensation.Benchmark.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AdaptiveBenchmarkMilestoneStoreChecks
{
    public static void MemoryAdaptiveBenchmarkMilestoneStoreFindsWhatItSaved()
    {
        var store = AdaptiveBenchmarkMilestoneStore.NewMemoryAdaptiveBenchmarkMilestoneStore();
        var item = AdaptiveBenchmarkMilestone.NewAdaptiveBenchmarkMilestone("code-682", "name-794");
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
        MemoryAdaptiveBenchmarkMilestoneStoreFindsWhatItSaved();
    }
}
