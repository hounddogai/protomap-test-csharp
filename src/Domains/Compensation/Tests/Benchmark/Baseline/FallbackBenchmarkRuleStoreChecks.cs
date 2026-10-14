namespace Acme.Workforce.Compensation.Tests.Benchmark.Baseline;

using Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackBenchmarkRuleStoreChecks
{
    public static void MemoryFallbackBenchmarkRuleStoreFindsWhatItSaved()
    {
        var store = FallbackBenchmarkRuleStore.NewMemoryFallbackBenchmarkRuleStore();
        var item = FallbackBenchmarkRule.NewFallbackBenchmarkRule("code-516", "label-470");
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
        MemoryFallbackBenchmarkRuleStoreFindsWhatItSaved();
    }
}
