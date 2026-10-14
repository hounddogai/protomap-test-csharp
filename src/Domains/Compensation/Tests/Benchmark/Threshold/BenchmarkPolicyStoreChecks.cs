namespace Acme.Workforce.Compensation.Tests.Benchmark.Threshold;

using Acme.Workforce.Compensation.Benchmark.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BenchmarkPolicyStoreChecks
{
    public static void MemoryBenchmarkPolicyStoreFindsWhatItSaved()
    {
        var store = BenchmarkPolicyStore.NewMemoryBenchmarkPolicyStore();
        var item = BenchmarkPolicy.NewBenchmarkPolicy("code-324", "name-385");
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
        MemoryBenchmarkPolicyStoreFindsWhatItSaved();
    }
}
