namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkBudgetStoreChecks
{
    public static void MemoryBenchmarkBudgetStoreFindsWhatItSaved()
    {
        var store = BenchmarkBudgetStore.NewMemoryBenchmarkBudgetStore();
        var item = BenchmarkBudget.NewBenchmarkBudget("code-521", "title-629");
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
        MemoryBenchmarkBudgetStoreFindsWhatItSaved();
    }
}
